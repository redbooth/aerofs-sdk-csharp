# AeroFS SDK in CSharp
This project aims to be the C\# SDK for [AeroFS
API](https://developers.aerofs.com/api).

## What are these files?
This directory contains 3 projects:

- AeroFSSDK: an API specification and a client implementation.
- AeroFSSDKDemoApp: A simple .NET MVC project as an example for how to use the SDK.
- AeroFSSDKTests: a test suite of system tests to run against an API endpoint.

## How to build?
### Debug Build
- Open AeroFSSDK solution.
- Go to top menu -> Tools -> NuGet Package Manager -> Manage NuGet Packages for
  Solution...
- The NuGet Package Manage dialog will open and and there should be a prompt at
  the top asking to install missing packages. Click Restore.
- Go to top menu -> Build -> Configuration Manager... and set Active Solution
  Configuration to Debug.
- Go to top menu -> Build -> Build Solution.
- This will produce a dll with debug symbols located in
  `AeroFSSDK/bin/Debug/AeroFSSDK.dll`.

### Release Build
- Download [nuget.exe](http://http://www.nuget.org/nuget.exe) and place it in
  `AeroFSSDK/.nuget`.
- Go to top menu -> Build -> Configuration Manager... and set Active Solution
  Configuration to Release.
- Go to top menu -> Build -> Build Solution.
- This will produce an optimized dll and a NuGet package (.nupkg) in
  `AeroFSSDK/bin/Release/`.
- Known bug: failure to build the NuGet package does not constitute a build
  failure and one cannot rebuild the solution unless one cleans the build or
  changes a file.

### Common Build Problems
- Missing references related to either log4net or Newtonsoft.Json.
  - Missing NuGet packages. See above instructions related to Manage NuGet
    Packages for Solution.
  - You may have to restart Visual Studio to get IntelliSense to pick up the
    new references.
- Release build fails with message relating to nuget and exit code 9009.
  - Missing `nuget.exe`. See above instruction related to download nuget.exe.
- Release build succeeded but still no NuGet packages in the output folder.
  - Creating NuGet package failed previously yet registered as a success.
    Clean and rebuild the solution.

## How to run system tests?
- Run an AeroFS appliance. **NOTE: It is _highly_ recommended that this appliance be a test appliance, because the user's files will be deleted and some of the tests require Organization Administrator privileges.**
- Obtain an access token to a user account. This can be done either via the
  [Authentication
  Workflow](https://developers.aerofs.com/api/en/1.3/#overview_authentication) or
  an user can create an auth token under personal Settings on the appliance's
  website.
- Obtain an access token with `org.admin` scope. See [OAuth Scopes](https://developers.aerofs.com/api/en/1.3/#oauth_scopes) for more information. The token generated in the previous step will not suffice, as it will not have the required scope. The tests in `AeroFSSDKTests/TestOAuth.cs` provide examples for how this can be done.
- Run an AeroFS client associated with the user account that created the access
  token and connect to the appliance above.
- **NOTE: as mentioned above, tests will delete all of a user's files. If you are not using a test appliance as recommended above, you should _at least_ use a test user account + client. You will still be unable to run tests requiring `org.admin` scope, unless you use an administrator account.**
- Determine the Host Name. e.g. `https://share.aerofs.com`.
- And the API Version you wish to target. e.g. `1.3`.
- Edit `AeroFSSDKTests/Settings.cs` and:
  - Put the Host Name (including `https://`) as the value for the HostName field.
  - Put the API Version as the value for the APIVersion field.
  - Put the access token as the value for the AccessToken field.
  - Put the organization admin access token as the value for the OrgAdminAccessToken field.
  - Put in any values (e.g. `test`) for the OAuth test fields (ClientID, ClientSecret, RedirectUri), unless you would like to use those tests to manually examine the OAuth flow, in which case you must configure and provide valid ClientID, ClientSecret and RedirectUri values that have been registered with the appliance.
  - Note that these values should not be disclosed or committed as they
    grant access to files stored on the user's AeroFS client.
- In Visual Studio, go to top menu -> Test -> Windows -> Test Explorer. When
  the Test Explorer opens, it should scan and pick up all system tests.
- Run individual, selected, or all tests using the Test Explorer.

### Common problems
- NullReferenceException or ArgumentException with creating client.
  - The HostName and/or AccessToken are not set in `Settings.cs`. See the above
    instruction related to editing `app.config`.
- Tests fail with 503 Service Unavailable.
  - Either AeroFS client or the appliance is not running. Start both the
    appliance and the client and keep them running.
- Tests fail with 502 Bad Gateway or connection refused.
  - The API client is experiencing issues connecting to the appliance.
    Ensure the EndPoint URL is correct, ends with a `/`, and the domain
    is pointed at the desired appliance.
  - Alternatively, investigate what's happening on the appliance if the API
    client is configured correctly.
- Tests fail with 401 Unauthorized.
  - The access token is either invalid or has expired. Obtain a new access
    token.
- Tests fail with 404 Not Found.
  - Ensure that the AeroFS client is connecting to the same appliance and the
    access token do grant access to the user account running the AeroFS
    client.
- Failure to create HttpWebRequest because of unable to establish trust.
  - The API client machine does not trust the certificate presented by the
    API endpoint. Either:
    - Provision the appliance or API endpoint with a globally trusted
      certificate signed by a trusted root CA.
    - Install the appliance's or API endpoint's certificate on the machine
      running the API client.
    - Or install the certificate of the root CA who signed the appliance's
      certificate on the machine running the API client.
- All of my files on my AeroFS client have disappeared.
  - You've been warned above that system tests will delete all files on the
    targetted client. You might be able to recover the files using [Sync
    History](https://support.aerofs.com/hc/en-us/articles/201439394-Sync-History).
- I've accidentally committed and published my API endpoint and access token,
  and now the Internet is stealing all my files. Halp!
  - The access token is compromised and the Internet does not forget.
    Nevertheless, you should take the following actions:
    - Revoke the offending access token on the appliance at `appliance_url/apps`.
    - Remove the offending commit from git.
    - Recover your lost files, if any, via Sync History.

## How to deploy / publish the release artifacts?
Either:

- Upload the NuGet package to a NuGet repository and import the package in the
  project using the AeroFS SDK.
- Copy the Release dll to the project using the AeroFS SDK and add it as a
  reference.
