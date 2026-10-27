using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Models;
using System.Diagnostics;

namespace OrbitAOS.V6.Controllers
{
    /// <summary>
    /// Home controller – handles the default landing page, privacy page, and error display.
    /// Upgraded to .NET 8 ASP.NET Core MVC conventions.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor injection of ILogger (registered automatically by the .NET 8 host).
        /// </summary>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET /Home/Index (default route)
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET /Home/Privacy
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// GET /Home/Error
        /// ResponseCache is set to NoStore so that error pages are never cached.
        /// Activity.Current?.Id provides the distributed trace identifier when available;
        /// HttpContext.TraceIdentifier is the fallback for the current request.
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
