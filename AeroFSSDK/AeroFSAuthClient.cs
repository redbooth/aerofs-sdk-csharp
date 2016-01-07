using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    /// <summary>
    /// Provides an entry point to create a new AeroFS API Authorization client for OAuth2 flow.
    /// </summary>
    public class AeroFSAuthClient
    {
        /// <summary>
        /// Creates a new AeroFS API Authorization client based on the provided configuration.
        /// </summary>
        /// <param name="config">The configuration to provision the new client.</param>
        /// <returns>A new AeroFS API client.</returns>
        /// <exception cref="ArgumentException">If there are any invalid configuration values.</exception>
        /// <example>
        ///     var client = AeroFSClient.Create(new AeroFSClient.Configuration
        ///         {
        ///             EndPoint = "https://share.aerofs.com/api/v1.2/",
        ///             AccessToken = "00000000000000000000000000000000",
        ///         });
        /// </example>
        public static AeroFSAuthAPI Create(Configuration config)
        {
            config.ValidateConfiguration();

            return new Impl.AeroFSAuthClientImpl
            {
                HostName = config.HostName,
                ClientID = config.ClientID,
                ClientSecret = config.ClientSecret,
                RedirectUri = config.RedirectUri
            };
        }

        /// <summary>
        /// Encapsulates all credentials necessary for an app for OAuth flow using AeroFSAuthClient.
        /// These values must match the values registered for the app with the API endpoint.
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// The host name of the target AeroFS Appliance.
            /// </summary>
            /// <example>"https://share.aerofs.com"</example>
            public string HostName { get; set; }

            /// <summary>
            /// The app's Client ID registered with the API endpoint.
            /// </summary>
            public string ClientID { get; set; }

            /// <summary>
            /// The app's Client Secret associated with Client ID by API endpoint.
            /// </summary>
            public string ClientSecret { get; set; }

            /// <summary>
            /// The app's Redirect Uri associated with Client ID by API endpoint.
            /// </summary>
            public string RedirectUri { get; set; }

            /// <summary>
            /// Validate the credential values.
            /// </summary>
            /// <exception cref="ArgumentException">If there are any invalid values.</exception>
            public void ValidateConfiguration()
            {
                if (HostName.IsNullOrEmpty()) throw new ArgumentNullException("HostName cannot be null or empty.");
                if (ClientID.IsNullOrEmpty()) throw new ArgumentNullException("ClientID cannot be null or empty.");
                if (ClientSecret.IsNullOrEmpty()) throw new ArgumentNullException("ClientSecret cannot be null or empty.");
            }
        }
    }
}
