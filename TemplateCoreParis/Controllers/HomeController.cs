using System;
using System.Collections.Generic;
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

namespace TemplateCoreParis.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        public static IHostingEnvironment _wwwRoot;

        //IT-ORVAL=>Test-aje-jg->Aje group Paris
        private WatsonCredentials _credentials = new WatsonCredentials()
        {
            workspaceID = "ac6889fe-7f09-4f71-ac0e-4d8850b72d2f",
            username = "7ecc7e1d-b7a9-472b-9419-a7254411cdd5",
            password = "HQJwcbFZclYL"
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
                //IT-ORVAL=>Test-aje-jg->Aje group Paris ENGLISH
                _credentials.workspaceID = "f4462a3c-709b-4dbb-993c-7dc4d59b5f4d";
            }
            else
            {
                _credentials.workspaceID = "ac6889fe-7f09-4f71-ac0e-4d8850b72d2f";
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
                    _credentials.workspaceID = "f4462a3c-709b-4dbb-993c-7dc4d59b5f4d";
                }
                else
                {
                    _credentials.workspaceID = "ac6889fe-7f09-4f71-ac0e-4d8850b72d2f";
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
