using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Models;
using PostMatchSummary.Services;

namespace PostMatchSummary.Controllers
{
    public class HomeController : Controller
    {
        private readonly MatchCacheService _cache;

        public HomeController(MatchCacheService cache)
        {
            _cache = cache;
        }

        [Route("")]
        [Route("home")]
        public IActionResult Index()
        {
            ViewBag.MatchCache = _cache;
            return View();
        }

        [Route("home/error")]
        [Route("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        [Route("about")]
        public IActionResult About()
        {
            ViewBag.Title = "About";
            return View();
        }
    }
}