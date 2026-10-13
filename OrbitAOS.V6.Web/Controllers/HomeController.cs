using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Web.Models;
using System.Diagnostics;

namespace OrbitAOS.V6.Web.Controllers
{
    /// <summary>
    /// Home controller handling the main application pages.
    /// Migrated from legacy ASP.NET MVC HomeController to ASP.NET Core MVC.
    /// Key changes:
    /// - ILogger injected via constructor DI (replaces legacy static Logger)
    /// - IActionResult return type (same as MVC 5)
    /// - No System.Web dependencies
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays the home page with application overview.
        /// </summary>
        public IActionResult Index()
        {
            _logger.LogInformation("Home page accessed at {Time}", DateTime.UtcNow);
            return View();
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays error information.
        /// Replaces legacy HandleErrorAttribute and Application_Error in Global.asax.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
