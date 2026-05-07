using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Services;

namespace PostMatchSummary.Controllers
{
    [Route("api/players")]
    public class PlayerController : Controller
    {
        private readonly MatchCacheService _cache;

        public PlayerController(MatchCacheService cache) { _cache = cache; }

        [Route("")]
        [Route("list")]
        public IActionResult Index()
        {
            var playersWithMatches = _cache.GetAllPlayersWithMatches();
            return View(playersWithMatches);
        }

        [Route("{name}")]
        [Route("profile/{name}")]
        public IActionResult Details(string name)
        {
            var player = _cache.GetPlayerByName(name);
            if (player == null) return NotFound();
            return View(player);
        }

        [Route("statistics/{name}")]
        [Route("stats/{name}")]
        public IActionResult Stats(string name)
        {
            var player = _cache.GetPlayerByName(name);
            if (player == null) return NotFound();
            return View("Details", player);
        }
    }
}