using AeroFSSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AeroFSSDKDemoApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private static readonly AeroFSAuthAPI AuthClient;
        private static readonly AeroFSAuthClient.Configuration Config;

        private static readonly string[] Errors = new string[]
        {
            "Authentication failed. Please try again."
        };

        public static bool Authenticated { get; set; } = false;

        static AuthenticationController()
        {
            Config = new AeroFSAuthClient.Configuration
            {
                HostName = "",
                ClientID = "",
                ClientSecret = "",
                RedirectUri = "http://localhost:PORT_NUMBER/Authentication/Finish"
            };

            AuthClient = AeroFSAuthClient.Create(Config);
        }

        // GET: Authentication
        public ActionResult Start(int error = -1)
        {
            var scopes = new OAuthScope[] { OAuthScope.FilesRead };
            ViewBag.AuthUrl = AuthClient.GenerateAuthorizationUrl(scopes);
            ViewBag.HasError = false;

            if (error != -1)
            {
                Authenticated = false;
                ViewBag.ErrorMessage = Errors[error];
                ViewBag.HasError = true;
            }

            ViewBag.Authenticated = Authenticated;

            return View();
        }

        public ActionResult Finish(string code)
        {
            if (code == null)
            {
                return RedirectToAction("Start");
            }

            try {
                var accessToken = AuthClient.ExchangeAuthorizationCodeForAccessToken(code);
                Authenticated = true;
                TempData["accessToken"] = accessToken;
                return RedirectToAction("AccessTokenSetup", "Home");
            }
            catch (WebException)
            {
                return RedirectToAction("Start", new { error = 0 });
            }
        }
    }
}