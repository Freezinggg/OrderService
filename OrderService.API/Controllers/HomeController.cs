using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Models;

namespace OrderService.API.Controllers
{
    public class HomeController : Controller
    {
        public static int IndexHit = 0;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Message = "This is " + Environment.GetEnvironmentVariable("INSTANCE_NAME");
            IndexHit++;
            ViewBag.IndexHitCounter = IndexHit;
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
