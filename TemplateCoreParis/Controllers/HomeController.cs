using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using static IBM.VCA.Watson.Watson.WatsonConversationService;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using TemplateCoreParis.Helpers;
using Newtonsoft.Json;
using TemplateCoreParis.WebChat;
using TemplateCoreParis.Models;

namespace TemplateCoreParis.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        public static IHostingEnvironment _wwwRoot;

        //Org:IT-ORVAL=>Space:VCA=>Conversation-vca->VCA Chatbot
        private WatsonCredentials _credentials = new WatsonCredentials()
        {
            workspaceID = "af012d44-256c-43ef-abfc-60938316552c",
            username = "07ca2289-7747-47de-a5e2-8d2b84b02893",
            password = "4hTOaJ6T2Us4"
        };

        public HomeController(IHostingEnvironment environment,
            IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
            _wwwRoot = environment;
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            if (culture == "en")
            {
                //Org:IT-ORVAL=>Space:VCA=>Conversation-vca->VCA Chatbot
                _credentials.workspaceID = "af012d44-256c-43ef-abfc-60938316552c";
            }
            else
            {
                //Org:IT-ORVAL=>Space:VCA=>Conversation-vca->VCA Chatbot
                _credentials.workspaceID = "af012d44-256c-43ef-abfc-60938316552c";
            }

            HttpContext.Session.SetObjectAsJson("Watson", _credentials);

            return LocalRedirect(returnUrl);
        }


        public IActionResult Index()
        {
            var creds = HttpContext?.Session?.GetObjectFromJson<WatsonCredentials>("Watson");

            if (creds == null)
            {
                HttpContext.Session.SetObjectAsJson("Watson", _credentials);
            }

            var cookie = Request.Cookies[".AspNetCore.Culture"];

            if (!string.IsNullOrEmpty(cookie))
            {
                if (cookie == "c=en|uic=en")
                {
                    _credentials.workspaceID = "af012d44-256c-43ef-abfc-60938316552c";
                }
                else
                {
                    _credentials.workspaceID = "af012d44-256c-43ef-abfc-60938316552c";
                }

                HttpContext.Session.SetObjectAsJson("Watson", _credentials);
            }

            ViewData["Title"] = _localizer["Inicio"];


            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Plantilla VCA";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Estamos atentos para atenderlo!";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public JsonResult GetGoogleUserInfoTask(string userEmail)
        {
            var _userinfo = GoogleUser.GetGoogleUserInfo(userEmail);

            var json = JsonConvert.SerializeObject(_userinfo);

            return Json(json);
        }

        public JsonResult GetGoogleTokensTask(string userEmail)
        {
            var _userinfo = GoogleUser.GenerateVerificationCodes(userEmail);

            var json = JsonConvert.SerializeObject(_userinfo);

            return Json(json);
        }

    }
}
