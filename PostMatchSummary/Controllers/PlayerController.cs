using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostMatchSummary.Services;
using PostMatchSummary.Models;

namespace PostMatchSummary.Controllers
{
    [Route("api/players")]
    public class PlayerController : Controller
    {
        private readonly MatchCacheService _cache;
        private readonly PostMatchSummaryDbContext _dbContext;
        private readonly RiotService _riotService;

        public PlayerController(MatchCacheService cache, PostMatchSummaryDbContext dbContext, RiotService riotService)
        {
            _cache = cache;
            _dbContext = dbContext;
            _riotService = riotService;
        }

        [Route("")]
        [Route("list")]
        public IActionResult Index(string? search)
        {
            var query = _dbContext.Players.Include(p => p.Champion).Include(p => p.Match).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.SummonerName.ToLower().Contains(search.ToLower()));

            var players = query.OrderBy(p => p.SummonerName).ToList();
            return View(players);
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

        [Route("search-riot")]
        [HttpGet]
        public async Task<IActionResult> SearchRiot(string? riotId)
        {
            if (!string.IsNullOrWhiteSpace(riotId))
            {
                var result = await _riotService.SearchPlayerAsync(riotId.Trim(), count: 20);
                if (result != null)
                {
                    LoadPlayerSummaryStats(result.GameName);
                    return View("SearchRiotResult", result);
                }

                ModelState.AddModelError("", $"Player '{riotId}' not found. Check the Riot ID format: Name#TAG");
            }

            return View();
        }

        [Route("search-riot")]
        [HttpPost]
        public async Task<IActionResult> SearchRiotPost(string riotId)
        {
            if (string.IsNullOrWhiteSpace(riotId))
            {
                ModelState.AddModelError("", "Riot ID is required (format: Name#TAG)");
                return View("SearchRiot");
            }

            var result = await _riotService.SearchPlayerAsync(riotId.Trim(), count: 20);

            if (result == null)
            {
                ModelState.AddModelError("", $"Player '{riotId}' not found. Check the Riot ID format: Name#TAG");
                return View("SearchRiot");
            }

            LoadPlayerSummaryStats(result.GameName);
            return View("SearchRiotResult", result);
        }

        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Champions = _dbContext.Champions.ToList();
            ViewBag.Teams = _dbContext.Teams.ToList();
            ViewBag.Matches = _dbContext.Matches.ToList();
            ViewBag.Roles = System.Enum.GetValues(typeof(ChampionRole)).Cast<ChampionRole>().ToList();
            return View();
        }

        [Route("create")]
        [HttpPost]
        public IActionResult Create(Player model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Players.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Champions = _dbContext.Champions.ToList();
            ViewBag.Teams = _dbContext.Teams.ToList();
            ViewBag.Matches = _dbContext.Matches.ToList();
            ViewBag.Roles = System.Enum.GetValues(typeof(ChampionRole)).Cast<ChampionRole>().ToList();
            return View(model);
        }

        [Route("{id}/edit")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var player = _dbContext.Players.Include(p => p.Champion).FirstOrDefault(p => p.Id == id);
            if (player == null)
                return NotFound($"Player with ID {id} not found");

            ViewBag.Champions = _dbContext.Champions.OrderBy(c => c.Name).ToList();
            ViewBag.Teams = _dbContext.Teams.ToList();
            ViewBag.Matches = _dbContext.Matches.ToList();
            ViewBag.Roles = System.Enum.GetValues(typeof(ChampionRole)).Cast<ChampionRole>().ToList();
            return View(player);
        }

        [Route("{id}/edit")]
        [HttpPost]
        public IActionResult Edit(int id, Player model)
        {
            var player = _dbContext.Players.Find(id);
            if (player == null)
                return NotFound($"Player with ID {id} not found");

            if (ModelState.IsValid)
            {
                player.SummonerName = model.SummonerName;
                player.ChampionId = model.ChampionId;
                player.Kills = model.Kills;
                player.Deaths = model.Deaths;
                player.Assists = model.Assists;
                player.CS = model.CS;
                player.GoldEarned = model.GoldEarned;
                player.Win = model.Win;
                player.Role = model.Role;
                player.TeamId = model.TeamId;
                player.MatchId = model.MatchId;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Champions = _dbContext.Champions.ToList();
            ViewBag.Teams = _dbContext.Teams.ToList();
            ViewBag.Matches = _dbContext.Matches.ToList();
            ViewBag.Roles = System.Enum.GetValues(typeof(ChampionRole)).Cast<ChampionRole>().ToList();
            return View(player);
        }

        [Route("{id}/delete")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var player = _dbContext.Players.Include(p => p.Champion).Include(p => p.Match).FirstOrDefault(p => p.Id == id);
            if (player == null)
                return NotFound($"Player with ID {id} not found");

            return View(player);
        }

        [Route("{id}/delete")]
        [HttpPost]
        public IActionResult DeleteConfirm(int id)
        {
            var player = _dbContext.Players.Find(id);
            if (player == null)
                return NotFound($"Player with ID {id} not found");

            _dbContext.Players.Remove(player);
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [Route("search")]
        [HttpGet]
        public IActionResult Search(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var players = _dbContext.Players.Include(p => p.Champion)
                .Where(p => p.SummonerName.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.SummonerName)
                .Take(10)
                .Select(p => new { id = p.Id, name = p.SummonerName, champion = p.Champion!.Name })
                .ToList();

            return Json(players);
        }

        // Preview — dohvati player bez spremanja, s matchId za highlight
        [Route("preview")]
        [HttpGet]
        public async Task<IActionResult> SearchRiotPreview(string? riotId, string? highlightMatch)
        {
            if (string.IsNullOrWhiteSpace(riotId))
                return RedirectToAction(nameof(SearchRiot));

            var result = await _riotService.SearchPlayerAsync(riotId.Trim(), count: 20, saveToDb: false);

            if (result == null)
            {
                ModelState.AddModelError("", $"Player '{riotId}' not found.");
                return View("SearchRiot");
            }

            ViewBag.IsPreview = true;
            ViewBag.HighlightMatch = highlightMatch; // matchId koji treba highlightati
            return View("SearchRiotResult", result);
        }

        [Route("{id}/match/{matchId}")]
        [HttpGet]
        public IActionResult MatchStats(int id, int matchId)
        {
            var player = _dbContext.Players
                .Include(p => p.Champion)
                .Include(p => p.Match)
                .Include(p => p.Team)
                .FirstOrDefault(p => p.Id == id && p.MatchId == matchId);

            if (player == null)
                return NotFound();

            return View(player);
        }

        private void LoadPlayerSummaryStats(string gameName)
        {
            var dbMatches = _dbContext.Players
                .Include(p => p.Match)
                .Include(p => p.Champion)
                .Include(p => p.Team)
                .Where(p => p.SummonerName == gameName)
                .ToList();

            ViewBag.DbTotalMatches = dbMatches.Count;
            ViewBag.DbWins = dbMatches.Count(p => p.Win);
            ViewBag.DbLosses = dbMatches.Count(p => !p.Win);
            ViewBag.DbAvgKills = dbMatches.Any() ? dbMatches.Average(p => p.Kills) : 0;
            ViewBag.DbAvgDeaths = dbMatches.Any() ? dbMatches.Average(p => p.Deaths) : 0;
            ViewBag.DbAvgAssists = dbMatches.Any() ? dbMatches.Average(p => p.Assists) : 0;
            ViewBag.DbAvgCS = dbMatches.Any() ? dbMatches.Average(p => p.CS) : 0;
            ViewBag.DbAvgGold = dbMatches.Any() ? dbMatches.Average(p => p.GoldEarned) : 0;

            var avgDeaths = dbMatches.Any() ? dbMatches.Average(p => p.Deaths) : 0;
            var avgKills = dbMatches.Any() ? dbMatches.Average(p => p.Kills) : 0;
            var avgAssists = dbMatches.Any() ? dbMatches.Average(p => p.Assists) : 0;

            ViewBag.DbAvgKda = avgDeaths == 0
                ? avgKills + avgAssists
                : (avgKills + avgAssists) / avgDeaths;

            ViewBag.DbWinRate = dbMatches.Count > 0
                ? (int)Math.Round((double)ViewBag.DbWins * 100 / dbMatches.Count)
                : 0;
        }
    }
}