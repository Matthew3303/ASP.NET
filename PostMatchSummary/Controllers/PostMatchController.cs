using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Services;
using PostMatchSummary.Models;
using System.Text.Json;

namespace PostMatchSummary.Controllers
{
    public class PostMatchController : Controller
    {
        private readonly RiotService _riotService;

        public PostMatchController(RiotService riotService)
        {
            _riotService = riotService;
        }

        public async Task<IActionResult> Index(string matchId)
        {
            var match = await _riotService.GetMatchAsync(matchId ?? "EUW1_7796654199");

            if (match == null)
                return Content("Greška pri dohvaćanju matcha");

            var summary = new MatchSummary
            {
                MatchId = match.MatchId,
                WinnerTeamName = match.WinnerTeam?.TeamName ?? "Nepoznato",
                TotalKills = match.TotalKills,
                DurationMinutes = match.Duration / 60,
                GameMode = match.GameMode
            };

            var topKiller = match.Players.OrderByDescending(p => p.Kills).FirstOrDefault();
            var topCS = match.Players.OrderByDescending(p => p.CS).FirstOrDefault();
            var bestKDA = match.Players
                .Where(p => p.Deaths > 0)
                .OrderByDescending(p => (double)(p.Kills + p.Assists) / p.Deaths)
                .FirstOrDefault();

            ViewBag.Summary = summary;
            ViewBag.TopKiller = topKiller;
            ViewBag.TopCS = topCS;
            ViewBag.BestKDA = bestKDA;

            return View(match);
        }

        public async Task<IActionResult> RawJson(string matchId)
        {
            var json = await _riotService.GetRawJsonAsync(matchId ?? "EUW1_7796654199");

            // Pretty print JSON
            var parsed = JsonSerializer.Deserialize<object>(json);
            var prettyJson = JsonSerializer.Serialize(parsed, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            ViewBag.MatchId = matchId ?? "EUW1_7796654199";
            return View("RawJson", prettyJson);
        }
    }
}