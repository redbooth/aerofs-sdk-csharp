using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AeroFSSDK.Impl
{
    internal partial class AeroFSClientImpl : AeroFSAPI
    {
        public string EndPoint { get; set; }
        public string AccessToken { get; set; }
        public int UploadChunkSize { get; set; }
        // TODO: figure out a better way to set the upload buffer.
        public byte[] UploadBuffer { get; set; }

        public File CreateFile(FolderID parent, string name)
        {
            var req = NewRequest("files");
            req.Method = "POST";
            req.ContentType = "application/json";

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });
            return ReadResponseBodyAndAssignETag<File>(req);
        }

        public Folder CreateFolder(FolderID parent, string name)
        {
            var req = NewRequest("folders");
            req.Method = "POST";
            req.ContentType = "application/json";

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });
            return ReadResponseBodyAndAssignETag<Folder>(req);
        }

        public void DeleteFile(FileID fileID)
        {
            var req = NewRequest("files/{0}".FormatWith(fileID.Base));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public void DeleteFolder(FolderID folderID)
        {
            var req = NewRequest("folders/{0}".FormatWith(folderID.Base));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public void ShareFolder(FolderID folderID)
        {
            var req = NewRequest("folders/{0}/is_shared".FormatWith(folderID.Base));
            req.Method = "PUT";
            req.GetResponse().Close();
        }

        public Folder MoveFolder(FolderID folderID, FolderID parent = null, string name = null, IList<string> etags = null)
        {
            var req = NewRequest("folders/{0}".FormatWith(folderID.Base));
            req.Method = "PUT";
            req.ContentType = "application/json";

            if (etags != null)
            {
                req.Headers["If-Match"] = CreateETagHeader(etags);
            }

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });
            return ReadResponseBodyAndAssignETag<Folder>(req);
        }

        public Stream DownloadFile(FileID fileID)
        {
            var req = NewRequest("files/{0}/content".FormatWith(fileID.Base));
            req.Method = "GET";
            return req.GetResponse().GetResponseStream();
        }

        public string FinishUpload(FileID fileID, UploadProgress progress)
        {
            var req = NewRequest("files/{0}/content".FormatWith(fileID.Base));
            req.Method = "PUT";
            req.Headers["Upload-ID"] = progress.UploadID.Base;
            req.Headers["Content-Range"] = "bytes */{0}".FormatWith(progress.BytesUploaded);

            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                return resp.Headers["ETag"].Trim('"');
            }
        }

        public File GetFile(FileID fileID, GetFileFields fields)
        {
            var queryFields = fields.Format("fields", GetFileFieldsMap);
            var req = NewRequest("files/{0}{1}".FormatWith(fileID.Base, queryFields));
            req.Method = "GET";
            return ReadResponseBodyToEnd<File>(req);
        }

        public ParentPath GetFilePath(FileID fileID)
        {
            var req = NewRequest("files/{0}/path".FormatWith(fileID.Base));
            req.Method = "GET";
            return ReadResponseBodyToEnd<ParentPath>(req);
        }

        public File MoveFile(FileID fileID, FolderID parent, string name, IList<string> etags = null)
        {
            var req = NewRequest("files/{0}".FormatWith(fileID.Base));
            req.Method = "PUT";
            req.ContentType = "application/json";

            if (etags != null)
            {
                req.Headers["If-Match"] = CreateETagHeader(etags);
            }

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });
            return ReadResponseBodyAndAssignETag<File>(req);
        }

        public Folder GetFolder(FolderID folderID, GetFolderFields fields)
        {
            var queryFields = fields.Format("fields", GetFolderFieldsMap);
            var req = NewRequest("folders/{0}{1}".FormatWith(folderID.Base, queryFields));
            req.Method = "GET";
            return ReadResponseBodyAndAssignETag<Folder>(req);
        }

        public Children ListChildren(FolderID folderID)
        {
            var req = NewRequest("folders/{0}/children".FormatWith(folderID.Base));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Children>(req);
        }

        public Children ListRoot()
        {
            return ListChildren(FolderID.Root);
        }

        public UploadProgress ResumeUpload(FileID fileID, UploadID uploadID)
        {
            var req = NewRequest("files/{0}/content".FormatWith(fileID.Base));
            req.Method = "PUT";
            req.Headers["Upload-ID"] = uploadID.Base;
            req.Headers["Content-Range"] = "bytes */*";

            try
            {
                return ReadUploadProgressFromResponse(req);
            }
            catch (WebException e)
            {
                // expected, this occurs if no contents have been uploaded thus far
                if (e.Response is HttpWebResponse && (e.Response as HttpWebResponse).StatusCode == HttpStatusCode.BadRequest)
                {
                    return new UploadProgress
                    {
                        UploadID = new UploadID { Base = (e.Response as HttpWebResponse).Headers["Upload-ID"] },
                        BytesUploaded = 0,
                        EOFReached = false,
                    };
                }

                throw e;
            }
        }

        public UploadProgress StartUpload(FileID fileID, Stream content)
        {
            var bytesRead = content.Read(UploadBuffer, 0, UploadChunkSize);

            var req = NewRequest("files/{0}/content".FormatWith(fileID.Base));
            req.Method = "PUT";
            req.ContentType = "application/octet-stream";

            if (bytesRead == 0)
            {
                req.Headers["Content-Range"] = "bytes */*";

                var progress = ReadUploadProgressFromResponse(req);
                progress.EOFReached = true;
                return progress;
            }
            else
            {
                req.Headers["Content-Range"] = "bytes {0}-{1}/*".FormatWith(0, bytesRead - 1);

                using (var os = req.GetRequestStream())
                {
                    os.Write(UploadBuffer, 0, bytesRead);
                }

                return ReadUploadProgressFromResponse(req);
            }
        }

        public UploadProgress UploadContent(FileID fileID, UploadProgress progress, Stream content)
        {
            var bytesRead = content.Read(UploadBuffer, 0, UploadChunkSize);

            if (bytesRead == 0)
            {
                return new UploadProgress
                {
                    UploadID = progress.UploadID,
                    BytesUploaded = progress.BytesUploaded,
                    EOFReached = true,
                };
            }

            var req = NewRequest("files/{0}/content".FormatWith(fileID.Base));
            req.Method = "PUT";
            req.ContentType = "application/octet-stream";
            req.Headers["Upload-ID"] = progress.UploadID.Base;
            req.Headers["Content-Range"] = "bytes {0}-{1}/*".FormatWith(progress.BytesUploaded, progress.BytesUploaded + bytesRead - 1);

            using (var os = req.GetRequestStream())
            {
                os.Write(UploadBuffer, 0, bytesRead);
            }

            return ReadUploadProgressFromResponse(req);
        }

        public IList<Link> ListLinks(ShareID shareID)
        {
            var req = NewRequest("shares/{0}/urls".FormatWith(shareID));
            req.Method = "GET";
            var links = ReadResponseBodyToEnd<IDictionary<string, IList<Link>>>(req);
            return links["urls"];
        }

        public Link GetLinkInfo(ShareID shareID, LinkID key)
        {
            var req = NewRequest("shares/{0}/urls/{1}".FormatWith(shareID, key));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Link>(req);
        }

        public Link CreateLink(ObjectID soid, string password = null, bool? requireLogin = default(bool?), long? expires = default(long?))
        {
            var req = NewRequest("shares/{0}/urls".FormatWith(soid.ShareID));
            req.Method = "POST";
            req.ContentType = "application/json";

            var body = new Dictionary<string, object> { { "soid", soid.Base } };
            if (!password.IsNullOrEmpty()) { body["password"] = password; }
            if (requireLogin.HasValue) { body["require_login"] = requireLogin.Value; }
            if (expires.HasValue) { body["expires"] = expires.Value; }

            WriteRequestBodyJson(req, body);
            return ReadResponseBodyToEnd<Link>(req);
        }

        public void DeleteLink(ShareID shareID, LinkID key)
        {
            var req = NewRequest("shares/{0}/urls/{1}".FormatWith(shareID, key));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public Link UpdateLinkInfo(ShareID shareID, LinkID key, string password = null, bool? requireLogin = default(bool?), long? expires = default(long?))
        {
            var req = NewRequest("shares/{0}/urls/{1}".FormatWith(shareID, key));
            req.Method = "PUT";
            req.ContentType = "application/json";

            var body = new Dictionary<string, object>();
            if (!password.IsNullOrEmpty()) { body["password"] = password; }
            if (requireLogin.HasValue) { body["require_login"] = requireLogin.Value; }
            if (expires.HasValue) { body["expires"] = expires.Value; }

            WriteRequestBodyJson(req, body);
            return ReadResponseBodyToEnd<Link>(req);
        }

        public Link UpdateLinkPassword(ShareID shareID, LinkID key, string password)
        {
            var req = NewRequest("shares/{0}/urls/{1}/password".FormatWith(shareID, key));
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, password);
            return ReadResponseBodyToEnd<Link>(req);
        }

        public Link RemoveLinkPassword(ShareID shareID, LinkID key)
        {
            var req = NewRequest("shares/{0}/urls/{1}/password".FormatWith(shareID, key));
            req.Method = "DELETE";
            return ReadResponseBodyToEnd<Link>(req);
        }

        public Link UpdateLinkRequireLogin(ShareID shareID, LinkID key, bool requireLogin)
        {
            var req = NewRequest("shares/{0}/urls/{1}/require_login".FormatWith(shareID, key));
            req.Method = requireLogin ? "PUT" : "DELETE";
            return ReadResponseBodyToEnd<Link>(req);
        }

        public Link UpdateLinkExpiry(ShareID shareID, LinkID key, long expiry)
        {
            var req = NewRequest("shares/{0}/urls/{1}/expires".FormatWith(shareID, key));
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, expiry);
            return ReadResponseBodyToEnd<Link>(req);
        }

        public Link RemoveLinkExpiry(ShareID shareID, LinkID key)
        {
            var req = NewRequest("shares/{0}/urls/{1}/expires".FormatWith(shareID, key));
            req.Method = "DELETE";
            return ReadResponseBodyToEnd<Link>(req);
        }

        public User GetUserInfo(string email)
        {
            var req = NewRequest("users/{0}".FormatWith(email));
            req.Method = "GET";
            return ReadResponseBodyToEnd<User>(req);
        }

        public User CreateUser(string email, string firstName, string lastName)
        {
            var req = NewRequest("users");
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { email = email, first_name = firstName, last_name = lastName });
            return ReadResponseBodyToEnd<User>(req);
        }

        public User UpdateUser(string email, string firstName, string lastName)
        {
            var req = NewRequest("users/{0}".FormatWith(email));
            req.Method = "PUT";
            req.ContentType = "application/json";

            var body = new Dictionary<string, object>();
            if (!firstName.IsNullOrEmpty()) { body["first_name"] = firstName; }
            if (!lastName.IsNullOrEmpty()) { body["last_name"] = lastName; }

            WriteRequestBodyJson(req, body);
            return ReadResponseBodyToEnd<User>(req);
        }

        public UserPage ListUsers(int limit = 20, int after = -1, int before = -1)
        {
            var uri = "users?limit={0}".FormatWith(limit);

            uri += (after == -1) ? "" : "&after={0}".FormatWith(after);
            uri += (before == -1) ? "" : "&before={0}".FormatWith(before);

            var req = NewRequest(uri);
            req.Method = "GET";
            return ReadResponseBodyToEnd<UserPage>(req);
        }

        public void DeleteUser(string email)
        {
            var req = NewRequest("users/{0}".FormatWith(email));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public void ChangeUserPassword(string email, string password)
        {
            var req = NewRequest("users/{0}/password".FormatWith(email));
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, password);
            req.GetResponse().Close();
        }

        public void DisableUserPassword(string email)
        {
            var req = NewRequest("users/{0}/password".FormatWith(email));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public bool CheckUserTwoFactorAuthEnabled(string email)
        {
            var req = NewRequest("users/{0}/two_factor".FormatWith(email));
            req.Method = "GET";
            return ReadAnonymousObjectFromResponseBody(req, new { enforced = false }).enforced;
        }

        public void DisableUserTwoFactorAuth(string email)
        {
            var req = NewRequest("users/{0}/two_factor".FormatWith(email));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public Invitee GetInviteeInfo(string email)
        {
            var req = NewRequest("invitees/{0}".FormatWith(email));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Invitee>(req);
        }

        public Invitee CreateInvitation(string emailTo, string emailFrom)
        {
            var req = NewRequest("invitees");
            req.Method = "POST";
            req.ContentType = "application/json";
            var invitation = new { email_to = emailTo, email_from = emailFrom };
            WriteRequestBodyJson(req, invitation);
            return ReadResponseBodyToEnd<Invitee>(req);
        }

        public void DeleteInvitation(string email)
        {
            var req = NewRequest("invitees/{0}".FormatWith(email));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public SharedFolderList ListSharedFolders(string email, IList<string> etags = null)
        {
            var req = NewRequest("users/{0}/shares".FormatWith(email));
            if (etags != null)
            {
                req.Headers["If-None-Match"] = CreateETagHeader(etags);
            }
            req.Method = "GET";

            // The response body for this call is just a JSON array
            // So JsonConvert cannot automatically turn this into a SharedFolderList object
            string etag;
            var body = ReadResponseBodyAndReturnETag<IList<SharedFolder>>(req, out etag);

            var ret = new SharedFolderList();
            ret.ETag = etag;
            ret.SharedFolders = body;
            return ret;
        }

        public SharedFolder CreateSharedFolder(string name)
        {
            var req = NewRequest("shares");
            req.Method = "POST";
            req.ContentType = "application/json";
            var sharedFolder = new { name = name };
            WriteRequestBodyJson(req, sharedFolder);
            return ReadResponseBodyAndAssignETag<SharedFolder>(req);
        }

        public SharedFolder GetSharedFolder(ShareID sharedFolderID, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}".FormatWith(sharedFolderID.Base));
            if (etags != null)
            {
                req.Headers["If-None-Match"] = CreateETagHeader(etags);
            }
            req.Method = "GET";
            return ReadResponseBodyAndAssignETag<SharedFolder>(req);
        }

        public SFMemberList ListSFMembers(ShareID sharedFolderID, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}/members".FormatWith(sharedFolderID.Base));
            if(etags != null)
            {
                req.Headers["If-None_Match"] = CreateETagHeader(etags);
            }
            req.Method = "GET";

            // The response body for this call is just a JSON array
            // So JsonConvert cannot automatically turn this into a SharedFolderList object
            string etag;
            var body = ReadResponseBodyAndReturnETag<IList<SFMember>>(req, out etag);

            var ret = new SFMemberList();
            ret.ETag = etag;
            ret.SFMembers = body;
            return ret;
        }

        public SFMember GetSFMember(ShareID sharedFolderID, string email, IList<string> etags)
        {
            var req = NewRequest("shares/{0}/members/{1}".FormatWith(sharedFolderID.Base, email));
            if (etags != null)
            {
                req.Headers["If-None-Match"] = CreateETagHeader(etags);
            }
            req.Method = "GET";
            return ReadResponseBodyAndAssignETag<SFMember>(req);
        }

        public SFMember AddSFMemberToSharedFolder(ShareID sharedFolderID, string email, IList<Permission> permissions)
        {
            var req = NewRequest("shares/{0}/members".FormatWith(sharedFolderID.Base));
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { email = email, permissions = GetPermissionStrings(permissions) });
            return ReadResponseBodyToEnd<SFMember>(req);
        }

        public SFMember SetSFMemberPermissions(ShareID sharedFolderID, string email, IList<Permission> permissions, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}/members/{1}".FormatWith(sharedFolderID.Base, email));
            if (etags != null)
            {
                req.Headers["If-Match"] = CreateETagHeader(etags);
            }
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { permissions = GetPermissionStrings(permissions) });
            return ReadResponseBodyAndAssignETag<SFMember>(req);
        }

        public void RemoveMemberFromSharedFolder(ShareID sharedFolderID, string email, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}/members/{1}".FormatWith(sharedFolderID.Base, email));
            if(etags != null)
            {
                req.Headers["If-Match"] = CreateETagHeader(etags);
            }
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        private HttpWebRequest NewRequest(string path)
        {
            var req = (HttpWebRequest)WebRequest.Create("{0}/{1}".FormatWith(EndPoint, path));
            req.Headers["Authorization"] = "Bearer {0}".FormatWith(AccessToken);
            return req;
        }

        private void WriteRequestBodyJson(HttpWebRequest req, object data)
        {
            using (var writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write(JsonConvert.SerializeObject(data));
            }
        }

        private T ReadResponseBodyAndAssignETag<T>(HttpWebRequest req) where T : class, WithETag
        {
            string etag;
            var body = ReadResponseBodyAndReturnETag<T>(req, out etag);
            body.ETag = etag;
            return body;
        }

        private T ReadResponseBodyAndReturnETag<T>(HttpWebRequest req, out string etag) where T : class
        {
            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                // ETag was provided in request and matched server-side, object not updated
                if (resp.StatusCode == HttpStatusCode.NotModified)
                {
                    etag = null;
                    return null;
                }

                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    etag = resp.Headers["ETag"];
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), SerializerSettings);
                }
            }
        }

        private T ReadResponseBodyToEnd<T>(HttpWebRequest req)
        {
            using (var resp = req.GetResponse())
            {
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), SerializerSettings);
                }
            }
        }

        private T ReadAnonymousObjectFromResponseBody<T>(HttpWebRequest req, T anonymousTypeObject)
        {
            using (var resp = req.GetResponse())
            {
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    return JsonConvert.DeserializeAnonymousType(reader.ReadToEnd(), anonymousTypeObject);
                }
            }
        }

        private UploadProgress ReadUploadProgressFromResponse(HttpWebRequest req)
        {
            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                return new UploadProgress
                {
                    UploadID = new UploadID { Base = resp.Headers["Upload-ID"] },
                    BytesUploaded = GetBytesUploadedFromResponse(resp),
                    EOFReached = false,
                };
            }
        }

        private long GetBytesUploadedFromResponse(HttpWebResponse resp)
        {
            if (!resp.Headers.AllKeys.Contains("Range")) return 0;
            string range = resp.Headers["Range"];
            return long.Parse(range.Substring(range.IndexOf('-') + 1).Trim()) + 1;
        }

        private string CreateETagHeader(IList<string> etags)
        {
            return String.Join(", ", etags.ToArray());
        }

        private IList<string> GetPermissionStrings(IList<Permission> permissions)
        {
            return permissions.Select(p => p.ToString().ToUpper()).ToArray();
        }

        private IDictionary<GetFileFields, string> GetFileFieldsMap { get; }
            = new Dictionary<GetFileFields, string>
            {
                { GetFileFields.Path, "path" },
            };

        private IDictionary<GetFolderFields, string> GetFolderFieldsMap { get; }
            = new Dictionary<GetFolderFields, string>
            {
                { GetFolderFields.Path, "path" },
                { GetFolderFields.Children, "children" },
            };
    }
}
