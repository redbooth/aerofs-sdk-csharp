using AeroFSSDK;
using AeroFSSDKDemoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AeroFSSDKDemoApp.Controllers
{
    public class HomeController : Controller
    {
        private static AeroFSAPI Client { get; set; }
        private static string SessionAccessToken { get; set; }

        // GET: Home
        public ActionResult Index(string folderID)
        {
            if (!AuthenticationController.Authenticated)
            {
                return RedirectToAction("Start", "Authentication");
            }

            var model = new HomeModel();
            var curFolderID = new FolderID { Base = folderID };

            var folderFields = GetFolderFields.Path;
            var curFolderMetadata = MakeAPICall(() => Client.GetFolder(curFolderID, folderFields)).Result;
            if (curFolderMetadata == null)
            {
                curFolderID = FolderID.Root;
                curFolderMetadata = MakeAPICall(() => Client.GetFolder(curFolderID, folderFields)).Result;
            }

            model.Path = curFolderMetadata.Path;

            var children = MakeAPICall(() => Client.ListChildren(curFolderID)).Result;
            if (children != null)
            {
                model.Files = children.Files;
                model.Folders = children.Folders;
            }
            model.CurFolder = curFolderMetadata;

            return View(model);
        }

        public ActionResult AccessTokenSetup()
        {
            SessionAccessToken = (string)TempData["accessToken"];
            if (SessionAccessToken != null)
            {
                var config = new AeroFSClient.Configuration
                {
                    HostName = "",
                    APIVersion = ""
                };

                try
                {
                    Client = AeroFSClient.Create(SessionAccessToken, config);
                }
                catch (ArgumentNullException)
                {
                    AuthenticationController.Authenticated = false;
                    return RedirectToAction("Start", "Authentication", new { error = 0 });
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private APIResult<T> MakeAPICall<T>(Func<T> apiCall)
        {
            var apiResult = new APIResult<T>();
            try
            {
                apiResult.Failed = false;
                apiResult.Result = apiCall();
            }
            catch (WebException e)
            {
                apiResult.Failed = true;
                if (e.Response is HttpWebResponse)
                {
                    apiResult.ErrorCode = (e.Response as HttpWebResponse).StatusCode;
                }
            }

            return apiResult;
        }
    }

    public class APIResult<T>
    {
        public bool Failed { get; set; }
        public HttpStatusCode ErrorCode { get; set; }
        public T Result { get; set; }
    }
}