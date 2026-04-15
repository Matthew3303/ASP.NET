using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Services;
using PostMatchSummary.Models;
using System.Text.Json;

namespace PostMatchSummary.Controllers
{
    public class PostMatchController : Controller
    {
        private readonly RiotService _riotService;
        private readonly MatchCacheService _cacheService;

        public PostMatchController(RiotService riotService, MatchCacheService cacheService)
        {
            _riotService = riotService;
            _cacheService = cacheService;
        }

        public IActionResult Index()
        {
            // Lista svih matcheva
            var matches = _cacheService.GetAll();
            return View(matches);
        }

        public async Task<IActionResult> Details(string? matchId)
        {
            // Match ID je obavezan parametar
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID je obavezan parametar");

            // Prvo pokušaj dohvatiti iz cachea
            var match = _cacheService.GetById(matchId);

            // Ako nema u cacheu, dohvati iz API-a
            if (match == null)
            {
                match = await _riotService.GetMatchAsync(matchId);
                if (match != null)
                {
                    _cacheService.AddMatch(match);
                }
            }

            if (match == null)
                return NotFound($"Match '{matchId}' nije pronađen");

            var topKiller = match.Players.OrderByDescending(p => p.Kills).FirstOrDefault();
            var topCS = match.Players.OrderByDescending(p => p.CS).FirstOrDefault();
            var bestKDA = match.Players
                .Where(p => p.Deaths > 0)
                .OrderByDescending(p => (double)(p.Kills + p.Assists) / p.Deaths)
                .FirstOrDefault();

            ViewBag.TopKiller = topKiller;
            ViewBag.TopCS = topCS;
            ViewBag.BestKDA = bestKDA;

            return View(match);
        }

        public async Task<IActionResult> RawJson(string? matchId)
        {
            // Match ID je obavezan parametar
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID je obavezan parametar");

            var json = await _riotService.GetRawJsonAsync(matchId);

            // Pretty print JSON
            var parsed = JsonSerializer.Deserialize<object>(json);
            var prettyJson = JsonSerializer.Serialize(parsed, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            ViewBag.MatchId = matchId;
            return View("RawJson", prettyJson);
        }
    }
}