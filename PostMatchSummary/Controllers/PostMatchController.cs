using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Services;
using PostMatchSummary.Models;
using System.Text.Json;

namespace PostMatchSummary.Controllers
{
    [Route("api/matches")]
    public class PostMatchController : Controller
    {
        private readonly RiotService _riotService;
        private readonly MatchCacheService _cacheService;

        public PostMatchController(RiotService riotService, MatchCacheService cacheService)
        {
            _riotService = riotService;
            _cacheService = cacheService;
        }

        [Route("")]
        [Route("overview")]
        public IActionResult Index()
        {
            var matches = _cacheService.GetAll();
            return View(matches);
        }

        [Route("{matchId}")]
        public async Task<IActionResult> Details(string? matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID je obavezan parametar");

            var match = _cacheService.GetById(matchId);

            if (match == null)
            {
                match = await _riotService.GetMatchAsync(matchId);
                if (match != null)
                    _cacheService.AddMatch(match);
            }

            if (match == null)
                return NotFound($"Match '{matchId}' nije pronađen");

            ViewBag.TopKiller = match.Players.OrderByDescending(p => p.Kills).FirstOrDefault();
            ViewBag.TopCS = match.Players.OrderByDescending(p => p.CS).FirstOrDefault();
            ViewBag.BestKDA = match.Players
                .Where(p => p.Deaths > 0)
                .OrderByDescending(p => (double)(p.Kills + p.Assists) / p.Deaths)
                .FirstOrDefault();

            return View(match);
        }

        [Route("json/{matchId}")]
        [Route("raw-json/{matchId}")]
        public async Task<IActionResult> RawJson(string? matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID je obavezan parametar");

            var json = await _riotService.GetRawJsonAsync(matchId);
            var parsed = JsonSerializer.Deserialize<object>(json);
            var prettyJson = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });

            ViewBag.MatchId = matchId;
            return View("RawJson", prettyJson);
        }

        [Route("top")]
        [Route("trending")]
        public IActionResult TopMatches()
        {
            var matches = _cacheService.GetAll()
                .OrderByDescending(m => m.TotalKills)
                .Take(5)
                .ToList();
            return View("Index", matches);
        }
    }
}