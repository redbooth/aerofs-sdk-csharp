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

            string etag;
            var response = ReadResponseBodyAndReturnETag<File>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public Folder CreateFolder(FolderID parent, string name)
        {
            var req = NewRequest("folders");
            req.Method = "POST";
            req.ContentType = "application/json";

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });

            string etag;
            var response = ReadResponseBodyAndReturnETag<Folder>(req, out etag);
            response.ETag = etag;
            return response;
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

        public Folder MoveFolder(FolderID folderID, FolderID parent, string name, IList<string> etags = null)
        {
            var req = NewRequest("folders/{0}".FormatWith(folderID.Base));
            req.Method = "PUT";
            req.ContentType = "application/json";

            if (etags != null)
            {
                req.Headers["If-Match"] = CreateETagHeader(etags);
            }

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });

            string etag;
            var response = ReadResponseBodyAndReturnETag<Folder>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public Stream DownloadFileContent(FileID fileID)
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

            string etag;
            var response = ReadResponseBodyAndReturnETag<File>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public Folder GetFolder(FolderID folderID, GetFolderFields fields)
        {
            var queryFields = fields.Format("fields", GetFolderFieldsMap);
            var req = NewRequest("folders/{0}{1}".FormatWith(folderID.Base, queryFields));
            req.Method = "GET";

            string etag;
            var response = ReadResponseBodyAndReturnETag<Folder>(req, out etag);
            response.ETag = etag;
            return response;
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

        public UserPage ListUsers(int? limit = null, string after = null, string before = null)
        {
            var uri = "users";
            var queryParams = new List<string>();

            if (limit.HasValue)
            {
                queryParams.Add("limit={0}".FormatWith(limit.Value));
            }
            if (after != null)
            {
                queryParams.Add("after={0}".FormatWith(after));
            }
            if (before != null)
            {
                queryParams.Add("before={0}".FormatWith(before));
            }

            var queryString = String.Join("&", queryParams.ToArray());
            if (!queryString.IsNullOrEmpty())
            {
                uri += "?" + queryString;
            }

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

        public void UpdateUserPassword(string email, string password)
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

        public bool IsUserTwoFactorAuthEnabled(string email)
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

        public Invitee CreateInvitee(string emailTo, string emailFrom)
        {
            var req = NewRequest("invitees");
            req.Method = "POST";
            req.ContentType = "application/json";
            var invitee = new { email_to = emailTo, email_from = emailFrom };
            WriteRequestBodyJson(req, invitee);
            return ReadResponseBodyToEnd<Invitee>(req);
        }

        public void DeleteInvitee(string email)
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

            string etag;
            var response = ReadResponseBodyAndReturnETag<SharedFolder>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public SharedFolder GetSharedFolder(ShareID sharedFolderID, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}".FormatWith(sharedFolderID.Base));
            if (etags != null)
            {
                req.Headers["If-None-Match"] = CreateETagHeader(etags);
            }
            req.Method = "GET";

            string etag;
            var response = ReadResponseBodyAndReturnETag<SharedFolder>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public SFMemberList ListSFMembers(ShareID sharedFolderID, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}/members".FormatWith(sharedFolderID.Base));
            if(etags != null)
            {
                req.Headers["If-None-Match"] = CreateETagHeader(etags);
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

            string etag;
            var response = ReadResponseBodyAndReturnETag<SFMember>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public SFMember AddSFMemberToSharedFolder(ShareID sharedFolderID, string email, IEnumerable<Permission> permissions)
        {
            var req = NewRequest("shares/{0}/members".FormatWith(sharedFolderID.Base));
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { email = email, permissions = GetPermissionStrings(permissions) });
            return ReadResponseBodyToEnd<SFMember>(req);
        }

        public SFMember SetSFMemberPermissions(ShareID sharedFolderID, string email, IEnumerable<Permission> permissions, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}/members/{1}".FormatWith(sharedFolderID.Base, email));
            if (etags != null)
            {
                req.Headers["If-Match"] = CreateETagHeader(etags);
            }
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { permissions = GetPermissionStrings(permissions) });

            string etag;
            var response = ReadResponseBodyAndReturnETag<SFMember>(req, out etag);
            response.ETag = etag;
            return response;
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

        public SFGroupMemberList ListSFGroupMembers(ShareID sharedFolderID)
        {
            var req = NewRequest("shares/{0}/groups".FormatWith(sharedFolderID));
            req.Method = "GET";

            var ret = new SFGroupMemberList();
            ret.SFGroupMembers = ReadResponseBodyToEnd<IList<SFGroupMember>>(req);
            return ret;
        }

        public SFGroupMember GetSFGroupMember(ShareID sharedFolderID, GroupID groupID)
        {
            var req = NewRequest("shares/{0}/members/{1}".FormatWith(sharedFolderID, groupID));
            req.Method = "GET";
            return ReadResponseBodyToEnd<SFGroupMember>(req);
        }

        public SFGroupMember AddGroupToSharedFolder(ShareID sharedFolderID, GroupID groupID, IEnumerable<Permission> permissions)
        {
            var req = NewRequest("shares/{0}/groups".FormatWith(sharedFolderID));
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { id = groupID, permissions = GetPermissionStrings(permissions) });
            return ReadResponseBodyToEnd<SFGroupMember>(req);
        }

        public SFGroupMember UpdateGroupPermissions(ShareID sharedFolderID, GroupID groupID, IEnumerable<Permission> permissions)
        {
            var req = NewRequest("shares/{0}/groups/{1}".FormatWith(sharedFolderID, groupID));
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { permissions = GetPermissionStrings(permissions) });
            return ReadResponseBodyToEnd<SFGroupMember>(req);
        }

        public void RemoveGroupFromSharedFolder(ShareID sharedFolderID, GroupID groupID)
        {
            var req = NewRequest("shares/{0}/groups/{1}".FormatWith(sharedFolderID, groupID));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public SFPendingMemberList ListSFPendingMembers(ShareID sharedFolderID, IList<string> etags = null)
        {
            var req = NewRequest("shares/{0}/pending".FormatWith(sharedFolderID));
            if (etags != null)
            {
                req.Headers["If-None-Match"] = CreateETagHeader(etags);
            }
            req.Method = "GET";

            // The response body for this call is just a JSON array
            // So JsonConvert cannot automatically turn this into a SharedFolderList object
            string etag;
            var body = ReadResponseBodyAndReturnETag<IList<SFPendingMember>>(req, out etag);

            var ret = new SFPendingMemberList();
            ret.ETag = etag;
            ret.SFPendingMembers = body;
            return ret;
        }

        public SFPendingMember GetSFPendingMember(ShareID sharedFolderID, string email)
        {
            var req = NewRequest("shares/{0}/pending/{1}".FormatWith(sharedFolderID, email));
            req.Method = "GET";
            return ReadResponseBodyToEnd<SFPendingMember>(req);
        }

        public SFPendingMember InviteUserToSharedFolder(ShareID sharedFolderID, string email, IEnumerable<Permission> permissions, string note)
        {
            var req = NewRequest("shares/{0}/pending".FormatWith(sharedFolderID));
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { email = email, permissions = GetPermissionStrings(permissions), note = note });
            return ReadResponseBodyToEnd<SFPendingMember>(req);
        }

        public void RemoveSFPendingMember(ShareID sharedFolderID, string email)
        {
            var req = NewRequest("shares/{0}/pending/{1}".FormatWith(sharedFolderID, email));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public InvitationList ListInvitations(string email)
        {
            var req = NewRequest("users/{0}/invitations".FormatWith(email));
            req.Method = "GET";
            return ReadResponseBodyToEnd<InvitationList>(req);
        }

        public Invitation GetInvitation(string email, ShareID sharedFolderID)
        {
            var req = NewRequest("users/{0}/invitations/{1}".FormatWith(email, sharedFolderID));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Invitation>(req);
        }

        public SharedFolder AcceptInvitation(string email, ShareID sharedFolderID, bool? external = null)
        {
            // Nicer for the end user to work with true/false, but API takes 0, 1
            int externalVal = -1;
            if (external.HasValue)
            {
                externalVal = external.Value ? 1 : 0;
            }

            var uri = "users/{0}/invitations/{1}".FormatWith(email, sharedFolderID);

            if (externalVal != -1)
            {
                uri += "?external={0}".FormatWith(externalVal);
            }

            var req = NewRequest(uri);
            req.Method = "POST";

            string etag;
            var response = ReadResponseBodyAndReturnETag<SharedFolder>(req, out etag);
            response.ETag = etag;
            return response;
        }

        public void DeleteInvitation(string email, ShareID sharedFolderID)
        {
            var req = NewRequest("users/{0}/invitations/{1}".FormatWith(email, sharedFolderID));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public GroupList ListGroups(int? offset = null, int? numResults = null)
        {
            var uri = "groups";

            var queryParams = new List<string>();
            if (offset.HasValue)
            {
                queryParams.Add("offset={0}".FormatWith(offset.Value));
            }
            if (numResults.HasValue)
            {
                queryParams.Add("results={0}".FormatWith(numResults.Value));
            }

            var queryString = String.Join("&", queryParams.ToArray());
            if (!queryString.IsNullOrEmpty())
            {
                uri += "?" + queryString;
            }

            var req = NewRequest(uri);
            req.Method = "GET";

            var ret = new GroupList();
            ret.Groups = ReadResponseBodyToEnd<IList<Group>>(req);
            return ret;
        }

        public Group CreateGroup(string name)
        {
            var req = NewRequest("groups");
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { name = name });
            return ReadResponseBodyToEnd<Group>(req);
        }

        public Group GetGroup(GroupID groupID)
        {
            var req = NewRequest("groups/{0}".FormatWith(groupID));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Group>(req);
        }

        public void DeleteGroup(GroupID groupID)
        {
            var req = NewRequest("groups/{0}".FormatWith(groupID));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public GroupMemberList ListGroupMembers(GroupID groupID)
        {
            var req = NewRequest("groups/{0}/members".FormatWith(groupID));
            req.Method = "GET";

            var ret = new GroupMemberList();
            ret.GroupMembers = ReadResponseBodyToEnd<IList<GroupMember>>(req);
            return ret;
        }

        public GroupMember AddMemberToGroup(GroupID groupID, string email)
        {
            var req = NewRequest("groups/{0}/members".FormatWith(groupID));
            req.Method = "POST";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { email = email });
            return ReadResponseBodyToEnd<GroupMember>(req);
        }

        public GroupMember GetGroupMember(GroupID groupID, string email)
        {
            var req = NewRequest("groups/{0}/members/{1}".FormatWith(groupID, email));
            req.Method = "GET";
            return ReadResponseBodyToEnd<GroupMember>(req);
        }

        public void RemoveGroupMember(GroupID groupID, string email)
        {
            var req = NewRequest("groups/{0}/members/{1}".FormatWith(groupID, email));
            req.Method = "DELETE";
            req.GetResponse().Close();
        }

        public DeviceList ListDevices(string email)
        {
            var req = NewRequest("users/{0}/devices".FormatWith(email));
            req.Method = "GET";

            var ret = new DeviceList();
            ret.Devices = ReadResponseBodyToEnd<IList<Device>>(req);
            return ret;
        }

        public Device GetDevice(DeviceID deviceID)
        {
            var req = NewRequest("devices/{0}".FormatWith(deviceID));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Device>(req);
        }

        public Device UpdateDevice(DeviceID deviceID, string name)
        {
            var req = NewRequest("devices/{0}".FormatWith(deviceID));
            req.Method = "PUT";
            req.ContentType = "application/json";
            WriteRequestBodyJson(req, new { name = name });
            return ReadResponseBodyToEnd<Device>(req);
        }

        public DeviceStatus GetDeviceStatus(DeviceID deviceID)
        {
            var req = NewRequest("devices/{0}/status".FormatWith(deviceID));
            req.Method = "GET";
            return ReadResponseBodyToEnd<DeviceStatus>(req);
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

        private IList<string> GetPermissionStrings(IEnumerable<Permission> permissions)
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
