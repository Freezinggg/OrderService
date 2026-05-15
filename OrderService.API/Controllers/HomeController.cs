using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Models;
using OrderService.Application.Interface.Cache;

namespace OrderService.API.Controllers
{
    public class HomeController : Controller
    {
        public static int IndexHit = 0;
        private readonly ILogger<HomeController> _logger;
        private readonly ISharedCounterCache _cache;

        public HomeController(ILogger<HomeController> logger, ISharedCounterCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Message = "This is " + Environment.GetEnvironmentVariable("INSTANCE_NAME");
            return View();
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
