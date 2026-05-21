using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostMatchSummary.Models;

namespace PostMatchSummary.Controllers
{
    [Route("api/teams")]
    public class TeamController : Controller
    {
        private readonly PostMatchSummaryDbContext _dbContext;

        public TeamController(PostMatchSummaryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /api/teams
        [Route("")]
        [Route("list")]
        [HttpGet]
        public IActionResult Index(string? search)
        {
            var query = _dbContext.Teams.Include(t => t.Match).Include(t => t.Players).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.TeamName.Contains(search, StringComparison.OrdinalIgnoreCase));

            return View(query.OrderBy(t => t.Id).ToList());
        }

        // GET: /api/teams/create
        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Matches = _dbContext.Matches.ToList();
            return View();
        }

        // POST: /api/teams/create
        [Route("create")]
        [HttpPost]
        public IActionResult Create(Team model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Teams.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Matches = _dbContext.Matches.ToList();
            return View(model);
        }

        // GET: /api/teams/{id}/edit
        [Route("{id}/edit")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var team = _dbContext.Teams.Include(t => t.Match).FirstOrDefault(t => t.Id == id);
            if (team == null)
                return NotFound($"Team with ID {id} not found");

            ViewBag.Matches = _dbContext.Matches.ToList();
            return View(team);
        }

        // POST: /api/teams/{id}/edit
        [Route("{id}/edit")]
        [HttpPost]
        public IActionResult Edit(int id, Team model)
        {
            var team = _dbContext.Teams.Find(id);
            if (team == null)
                return NotFound($"Team with ID {id} not found");

            if (ModelState.IsValid)
            {
                team.TeamId = model.TeamId;
                team.Win = model.Win;
                team.MatchId = model.MatchId;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Matches = _dbContext.Matches.ToList();
            return View(team);
        }

        // GET: /api/teams/{id}/delete
        [Route("{id}/delete")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var team = _dbContext.Teams.Include(t => t.Players).Include(t => t.Match).FirstOrDefault(t => t.Id == id);
            if (team == null)
                return NotFound($"Team with ID {id} not found");

            return View(team);
        }

        // POST: /api/teams/{id}/delete
        [Route("{id}/delete")]
        [HttpPost]
        public IActionResult DeleteConfirm(int id)
        {
            var team = _dbContext.Teams.Find(id);
            if (team == null)
                return NotFound($"Team with ID {id} not found");

            var playersCount = _dbContext.Players.Count(p => p.TeamId == id);
            if (playersCount > 0)
            {
                ModelState.AddModelError("", $"Cannot delete team — it contains {playersCount} player(s)");
                return View("Delete", team);
            }

            _dbContext.Teams.Remove(team);
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // AJAX: /api/teams/search?q=
        [Route("search")]
        [HttpGet]
        public IActionResult Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var teams = _dbContext.Teams
                .Where(t => t.TeamName.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.TeamName)
                .Take(10)
                .Select(t => new { id = t.Id, name = t.TeamName })
                .ToList();

            return Json(teams);
        }
    }
}