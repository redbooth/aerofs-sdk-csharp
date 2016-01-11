using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK.Tests
{
    public class Settings
    {
        // Need to provide values here to configure tests
        // The host name of the appliance against which you want to run tests
        // e.g. "https://share.aerofs.com"
        public const string HostName = "https://share.syncfs.com";

        // The target API version of the appliance, e.g. "1.4"
        public const string APIVersion = "1.4";

        // NOTE: tests will wipe the AeroFS folder for the user against which tests are run
        // A valid access token to run file/folder etc. tests with
        // Can be obtained through Settings menu > API Access Tokens in the Appliance
        // Token should NOT have organization.admin scope (which is the case by default)
        // If tests are failing with a 503 HTTP status, check if this is the case
        // e.g. "9dk4955c0407459d97b2c833ce08dbe3"
        public const string AccessToken = "9cf4955c0407459d97b2c833ce08dbe3";

        // A valid ORG ADMIN access token to run tests with
        // Obtain by completing OAuth flow with the 'organization.admin' scope (and all others)
        // Token must have organization.admin scope to run certain tests
        // If tests are failing with permission errors, check that this token has proper scopes
        // e.g. "9dk4955c0407459d97b2c833ce08dbe3"
        public const string OrgAdminAccessToken = "fecc9b0da0b94f3cb6dafc73988b0efc";
    }
}
