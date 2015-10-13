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
    internal class AeroFSClientImpl : AeroFSAPI
    {
        public string EndPoint { get; set; }
        public string AccessToken { get; set; }
        public int UploadChunkSize { get; set; }
        // TODO: figure out a better way to set the upload buffer.
        public byte[] UploadBuffer { get; set; }

        private JsonSerializerSettings SerializerSettings { get; } 
            = new JsonSerializerSettings
            {
                Converters = new JsonConverter[]
                {
                    new StringIDReader<FileID>(),
                    new StringIDReader<FolderID>(),
                    new StringIDReader<ShareID>(),
                    new StringIDReader<UploadID>(),
                },
                ContractResolver = new DelegateContractResolver
                {
                    Resolvers = new Dictionary<Type, IContractResolver>
                    {
                        { typeof(Folder), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "ID", "id" },
                                { "Name", "name" },
                                { "Parent", "parent" },
                                { "IsShared", "is_shared" },
                                { "SID", "sid" },
                                { "Path", "path" },
                                { "Children", "children" },
                            },
                        } },
                        { typeof(File), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "ID", "id" },
                                { "Name", "name" },
                                { "Parent", "parent" },
                                { "LastModified", "last_modified" },
                                { "Size", "size" },
                                { "MimeType", "mime_type" },
                                { "ETag", "etag" },
                                { "Path", "path" },
                            },
                        } },
                    },
                },
            };

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

        public File CreateFile(FolderID parent, string name)
        {
            var req = NewRequest("files");
            req.Method = "POST";
            req.ContentType = "application/json";

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });
            return ReadResponseBodyToEnd<File>(req);
        }

        public Folder CreateFolder(FolderID parent, string name)
        {
            var req = NewRequest("folders");
            req.Method = "POST";
            req.ContentType = "application/json";

            WriteRequestBodyJson(req, new { parent = parent.Base, name = name });
            return ReadResponseBodyToEnd<Folder>(req);
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

        public Folder GetFolder(FolderID folderID, GetFolderFields fields)
        {
            var queryFields = fields.Format("fields", GetFolderFieldsMap);
            var req = NewRequest("folders/{0}{1}".FormatWith(folderID.Base, queryFields));
            req.Method = "GET";
            return ReadResponseBodyToEnd<Folder>(req);
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
    }
}
