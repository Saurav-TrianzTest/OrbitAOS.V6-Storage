using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Models;
using System.Diagnostics;

// ============================================================
// HomeController.cs – Web (Presentation) Layer
// Clean Architecture: Controllers delegate to Application layer
// services via constructor-injected interfaces.
//
// Migration notes (.NET 6 → .NET 10):
//   - IActionResult return types are unchanged
//   - ILogger<T> injection pattern is unchanged
//   - [ResponseCache] replaces legacy [OutputCache] attribute
//   - Async actions preferred; add async/await for I/O-bound work
// ============================================================

namespace OrbitAOS.V6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // In Clean Architecture, inject Application-layer service interfaces here.
        // Example: private readonly IHomeService _homeService;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET /Home/Index  (or /)
        // Async pattern preferred in .NET 10 for I/O-bound operations.
        public async Task<IActionResult> Index()
        {
            // Example: var model = await _homeService.GetDashboardDataAsync();
            // return View(model);
            await Task.CompletedTask; // placeholder – remove when real async work is added
            return View();
        }

        // GET /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET /Home/Error
        // [ResponseCache] replaces legacy [OutputCache(NoStore = true, Duration = 0)]
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
