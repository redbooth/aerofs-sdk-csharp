using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    /// <summary>
    /// A specification of the AeroFS Authorization API accessible through an Auth client.
    /// </summary>
    public interface AeroFSAuthAPI
    {
        /// <summary>
        /// Generates URL to proceed with authorization flow.
        /// </summary>
        /// <param name="scopes">List of requested OAuth scopes.</param>
        /// <returns>A URL to which the end user can be redirected to start the OAuth flow.</returns>
        string GenerateAuthorizationUrl(IEnumerable<OAuthScope> scopes);

        /// <summary>
        /// Exchanges an OAuthv2 authorization code for an access token to be used in future API requests.
        /// </summary>
        /// <returns>The new access token, which can be used for AeroFSClient.Create()</returns>
        string ExchangeAuthorizationCodeForAccessToken(string code);

        /// <summary>
        /// Revokes an access token so that it can no longer be used to make requests on the user's behalf.
        /// </summary>
        /// <param name="accessToken">The access token to be revoked.</param>
        void RevokeAccessToken(string accessToken);
    }

    public enum OAuthScope
    {
        FilesRead,
        FilesWrite,
        FilesAppdata,
        UserRead,
        UserWrite,
        UserPassword,
        ACLRead,
        ACLWrite,
        ACLInvitations,
        GroupsRead,
        OrganizationAdmin
    }
}
