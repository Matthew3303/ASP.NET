using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Services;

namespace PostMatchSummary.Controllers
{
    public class PlayerController : Controller
    {
        private readonly MatchCacheService _cache;
        public PlayerController(MatchCacheService cache) { _cache = cache; }

        public IActionResult Index()
        {
            var playersWithMatches = _cache.GetAllPlayersWithMatches();
            return View(playersWithMatches);
        }

        public IActionResult Details(string name)
        {
            var player = _cache.GetPlayerByName(name);
            if (player == null) return NotFound();
            return View(player);
        }
    }
}