using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TemplateCoreParis.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
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
    }
}
