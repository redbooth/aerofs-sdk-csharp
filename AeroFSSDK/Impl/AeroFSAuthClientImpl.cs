using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AeroFSSDK.Impl
{
    internal class AeroFSAuthClientImpl : AeroFSAuthAPI
    {
        public string HostName { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }

        private IDictionary<OAuthScope, string> scopeNameMap = new Dictionary<OAuthScope, string>
        {
            [OAuthScope.FilesRead] = "files.read",
            [OAuthScope.FilesWrite] = "files.write",
            [OAuthScope.FilesAppdata] = "files.appdata",
            [OAuthScope.UserRead] = "user.read",
            [OAuthScope.UserWrite] = "user.write",
            [OAuthScope.UserPassword] = "user.password",
            [OAuthScope.ACLRead] = "acl.read",
            [OAuthScope.ACLWrite] = "acl.write",
            [OAuthScope.ACLInvitations] = "acl.invitations",
            [OAuthScope.OrganizationAdmin] = "organization.admin"
        };

        public string GenerateAuthorizationUrl(OAuthScope[] scopes)
        {
            string[] scopesAsStrings = scopes.Select<OAuthScope, string>(scope => scopeNameMap[scope]).ToArray();
            string scopeString = String.Join(",", scopesAsStrings);

            return
                "{0}/authorize?response_type=code".FormatWith(HostName) +
                "&client_id={0}".FormatWith(ClientID) +
                "&redirect_uri={0}".FormatWith(RedirectUri) +
                "&scope={0}".FormatWith(scopeString);
        }

        public string GenerateAuthorizationUrl(OAuthScope scope)
        {
            return GenerateAuthorizationUrl(new OAuthScope[] { scope });
        }

        public string ExchangeAuthorizationCodeForAccessToken(string code)
        {
            var uri =
                "{0}/auth/token".FormatWith(HostName);

            var data =
                "grant_type=authorization_code" +
                "&code={0}".FormatWith(code) +
                "&client_id={0}".FormatWith(ClientID) +
                "&client_secret={0}".FormatWith(ClientSecret) +
                "&redirect_uri={0}".FormatWith(RedirectUri);


            Console.WriteLine(uri);
            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            var bytes = System.Text.Encoding.ASCII.GetBytes(data);
            req.ContentLength = bytes.Length;
            var os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();


            using (var resp = req.GetResponse())
            {
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    var anonType = new { access_token = string.Empty, token_type = string.Empty, expires_in = 0, scope = string.Empty };
                    return JsonConvert.DeserializeAnonymousType(reader.ReadToEnd(), anonType).access_token;
                }
            }
        }
    }
}
