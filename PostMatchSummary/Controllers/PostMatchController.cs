using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostMatchSummary.Services;
using PostMatchSummary.Models;
using System.Text.Json;

namespace PostMatchSummary.Controllers
{
    [Route("api/matches")]
    public class PostMatchController : Controller
    {
        private readonly RiotService _riotService;
        private readonly PostMatchSummaryDbContext _dbContext;

        public PostMatchController(RiotService riotService, PostMatchSummaryDbContext dbContext)
        {
            _riotService = riotService;
            _dbContext = dbContext;
        }

        [Route("")]
        [Route("overview")]
        public IActionResult Index(string? search)
        {
            var query = _dbContext.Matches
                .Include(m => m.Teams)
                .Include(m => m.Players)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m =>
                    m.MatchId.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    m.GameMode.Contains(search, StringComparison.OrdinalIgnoreCase));

            return View(query.OrderByDescending(m => m.GameCreation).ToList());
        }

        [Route("{matchId}")]
        public async Task<IActionResult> Details(string matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID is required");

            var match = await _dbContext.Matches
                .Include(m => m.Teams).ThenInclude(t => t.Players).ThenInclude(p => p.Champion)
                .Include(m => m.Players).ThenInclude(p => p.Champion)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null)
            {
                match = await _riotService.GetMatchAsync(matchId);
                if (match == null)
                    return NotFound($"Match '{matchId}' not found");

                _dbContext.Matches.Add(match);
                await _dbContext.SaveChangesAsync();
            }

            ViewBag.TopKiller = match.Players.OrderByDescending(p => p.Kills).FirstOrDefault();
            ViewBag.TopCS = match.Players.OrderByDescending(p => p.CS).FirstOrDefault();
            ViewBag.BestKDA = match.Players
                .Where(p => p.Deaths > 0)
                .OrderByDescending(p => (double)(p.Kills + p.Assists) / p.Deaths)
                .FirstOrDefault();

            return View(match);
        }

        // Preview — dohvati iz Riot API ali NE spremi u DB
        [Route("preview/{matchId}")]
        public async Task<IActionResult> Preview(string matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID is required");

            // Ako već postoji u DB, prikaži normalno (nije preview)
            var match = await _dbContext.Matches
                .Include(m => m.Teams).ThenInclude(t => t.Players).ThenInclude(p => p.Champion)
                .Include(m => m.Players).ThenInclude(p => p.Champion)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null)
            {
                var json = await _riotService.GetRawJsonAsync(matchId);
                if (string.IsNullOrEmpty(json))
                    return NotFound($"Match '{matchId}' not found");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var riotResponse = JsonSerializer.Deserialize<RiotResponse>(json, options);
                if (riotResponse?.Info == null)
                    return NotFound($"Match '{matchId}' could not be parsed");

                match = await _riotService.MapToMatchAsyncPreview(riotResponse, _dbContext);
                if (match == null)
                    return NotFound($"Match '{matchId}' could not be mapped");

                ViewBag.IsPreview = true;
            }

            ViewBag.TopKiller = match.Players.OrderByDescending(p => p.Kills).FirstOrDefault();
            ViewBag.TopCS = match.Players.OrderByDescending(p => p.CS).FirstOrDefault();
            ViewBag.BestKDA = match.Players
                .Where(p => p.Deaths > 0)
                .OrderByDescending(p => (double)(p.Kills + p.Assists) / p.Deaths)
                .FirstOrDefault();

            return View("Details", match);
        }

        [Route("raw-json/{matchId}")]
        public async Task<IActionResult> RawJson(string matchId)
        {
            if (string.IsNullOrWhiteSpace(matchId))
                return NotFound("Match ID is required");

            var json = await _riotService.GetRawJsonAsync(matchId);
            var parsed = JsonSerializer.Deserialize<object>(json);
            var prettyJson = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });

            ViewBag.MatchId = matchId;
            return View("RawJson", prettyJson);
        }

        [Route("create")]
        [HttpGet]
        public IActionResult Create() => View();

        [Route("create")]
        [HttpPost]
        public IActionResult Create(Match model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Matches.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Route("{id:int}/edit")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var match = _dbContext.Matches.Find(id);
            if (match == null) return NotFound($"Match with ID {id} not found");
            return View(match);
        }

        [Route("{id:int}/edit")]
        [HttpPost]
        public IActionResult Edit(int id, Match model)
        {
            var match = _dbContext.Matches.Find(id);
            if (match == null) return NotFound($"Match with ID {id} not found");

            if (ModelState.IsValid)
            {
                match.MatchId = model.MatchId;
                match.GameCreation = model.GameCreation;
                match.Duration = model.Duration;
                match.GameMode = model.GameMode;
                match.GameVersion = model.GameVersion;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(match);
        }

        [Route("{id:int}/delete")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var match = _dbContext.Matches
                .Include(m => m.Teams)
                .Include(m => m.Players)
                .FirstOrDefault(m => m.Id == id);

            if (match == null) return NotFound($"Match with ID {id} not found");
            return View(match);
        }

        [Route("{id:int}/delete")]
        [HttpPost]
        public IActionResult DeleteConfirm(int id)
        {
            var match = _dbContext.Matches.Find(id);
            if (match == null) return NotFound($"Match with ID {id} not found");

            var teamsCount = _dbContext.Teams.Count(t => t.MatchId == id);
            var playersCount = _dbContext.Players.Count(p => p.MatchId == id);

            if (teamsCount > 0 || playersCount > 0)
            {
                ModelState.AddModelError("", $"Cannot delete match — it contains {teamsCount} team(s) and {playersCount} player(s)");
                return View("Delete", match);
            }

            _dbContext.Matches.Remove(match);
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [Route("search")]
        [HttpGet]
        public IActionResult Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var matches = _dbContext.Matches
                .Where(m => m.MatchId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                            m.GameMode.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(m => m.GameCreation)
                .Take(10)
                .Select(m => new
                {
                    id = m.Id,
                    matchId = m.MatchId,
                    gameMode = m.GameMode,
                    date = m.GameCreation.ToString("dd.MM.yyyy HH:mm")
                })
                .ToList();

            return Json(matches);
        }
    }
}