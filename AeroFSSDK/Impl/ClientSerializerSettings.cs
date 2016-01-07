using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK.Impl
{
    internal partial class AeroFSClientImpl
    {
        private JsonSerializerSettings SerializerSettings { get; }
            = new JsonSerializerSettings
            {
                Converters = new JsonConverter[]
                {
                    new StringIDReader<ObjectID>(),
                    new StringIDReader<FileID>(),
                    new StringIDReader<FolderID>(),
                    new StringIDReader<ShareID>(),
                    new StringIDReader<UploadID>(),
                    new StringIDReader<LinkID>(),
                    new StringIDReader<SFGroupMemberID>()
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
                                { "ShareID", "sid" },
                                { "Path", "path" },
                                { "Children", "children" },
                            }
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
                                { "ContentState", "content_state" }
                            }
                        } },
                        { typeof(Invitee), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "EmailTo", "email_to" },
                                { "EmailFrom", "email_from" },
                                { "SignUpCode", "signup_code" }
                            }
                        } },
                        { typeof(Link), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "Key", "key" },
                                { "ObjectID", "soid" },
                                { "Token", "token" },
                                { "CreatedBy", "created_by" },
                                { "RequireLogin", "require_login" },
                                { "HasPassword", "has_password" },
                                { "Expires", "expires" },
                            }
                        } },
                        { typeof(SFGroupMember), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "ID", "id" },
                                { "Name", "name" },
                                { "Permissions", "permissions" }
                            }
                        } },
                        { typeof(SFMember), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "Email", "email" },
                                { "FirstName", "first_name" },
                                { "LastName", "last_name" },
                                { "Permissions", "permissions" }
                            }
                        } },
                        { typeof(SFPendingMember), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "Email", "email" },
                                { "FirstName", "first_name" },
                                { "LastName", "last_name" },
                                { "InvitedBy", "invited_by" },
                                { "Permissions", "permissions" }
                            }
                        } },
                        { typeof(SharedFolder), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "ID", "id" },
                                { "Name", "name" },
                                { "IsExternal", "external" },
                                { "Members", "members" },
                                { "Groups", "groups" },
                                { "Pending", "pending" },
                                { "CallerEffectivePermissions", "caller_effective_permissions" }
                            }
                        } },
                        { typeof(User), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "Email", "email" },
                                { "FirstName", "first_name" },
                                { "LastName", "last_name" },
                            }
                        } },
                        { typeof(UserPage), new RemapPropertyNamesContractResolver
                        {
                            PropertyMapping = new Dictionary<string, string>
                            {
                                { "HasMore", "has_more" },
                                { "Users", "data" }
                            }
                        } },
                    },
                },
            };
    }
}
