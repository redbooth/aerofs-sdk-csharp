using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    /// <summary>
    /// Provides an entry point to create a new AeroFS API client.
    /// </summary>
    public static class AeroFSClient
    {
        /// <summary>
        /// Creates a new AeroFS API client based on the provided configuration.
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
        public static AeroFSAPI Create(Configuration config)
        {
            config.ValidateConfiguration();

            return new Impl.AeroFSClientImpl
            {
                EndPoint = config.EndPoint,
                AccessToken = config.AccessToken,
                UploadChunkSize = config.UploadChunkSize,
                // TODO: figure out a better way to provision this
                UploadBuffer = new byte[config.UploadChunkSize],
            };
        }

        /// <summary>
        /// Encapsulates all configuration values to create a new AeroFS API client.
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// The default value for <see cref="UploadChunkSize"/>.
            /// </summary>
            public const int DefaultChunkSize = 4096;

            /// <summary>
            /// The URI of the AeroFS API endpoint.
            /// </summary>
            /// <example>"https://share.aerofs.com/api/v1.2"</example>
            public string EndPoint { get; set; }

            /// <summary>
            /// The access token to use to authenticate the client to the API endpoint.
            /// </summary>
            public string AccessToken { get; set; }

            /// <summary>
            /// The maximum number of bytes to upload per request when uploading file content.
            /// </summary>
            public int UploadChunkSize { get; set; }
                = DefaultChunkSize;

            /// <summary>
            /// Validate the values in this configuration.
            /// </summary>
            /// <exception cref="ArgumentException">If there are any invalid values.</exception>
            public void ValidateConfiguration()
            {
                if (EndPoint.IsNullOrEmpty()) throw new ArgumentNullException("EndPoint cannot be null or empty.");
                if (AccessToken.IsNullOrEmpty()) throw new ArgumentNullException("AccessToken cannot be null or empty.");
                if (UploadChunkSize <= 0) throw new ArgumentOutOfRangeException("UploadChunkSize must be greater than 0.");
            }
        }
    }
}
