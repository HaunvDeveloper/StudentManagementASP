using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementASP.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace StudentManagementASP.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult _MenuAdmin()
        {
            return PartialView("_MenuAdmin");
        }

        public IActionResult _MenuLecturer()
        {
            return PartialView("_MenuLecturer");
        }

        public IActionResult _MenuStudent()
        {
            return PartialView("_MenuStudent");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
