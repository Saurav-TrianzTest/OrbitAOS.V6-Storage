using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OrbitAOS.V6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            await Task.CompletedTask;
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            await Task.CompletedTask;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            await Task.CompletedTask;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}