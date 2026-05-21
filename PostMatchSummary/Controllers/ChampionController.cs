using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostMatchSummary.Models;

namespace PostMatchSummary.Controllers
{
    [Route("api/champions")]
    public class ChampionController : Controller
    {
        private readonly PostMatchSummaryDbContext _dbContext;

        public ChampionController(PostMatchSummaryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /api/champions
        [Route("")]
        [Route("list")]
        [HttpGet]
        public IActionResult Index(string? search)
        {
            var query = _dbContext.Champions.Include(c => c.Players).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

            return View(query.OrderBy(c => c.Name).ToList());
        }

        // GET: /api/champions/create
        [Route("create")]
        [HttpGet]
        public IActionResult Create() => View();

        // POST: /api/champions/create
        [Route("create")]
        [HttpPost]
        public IActionResult Create(Champion model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Champions.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /api/champions/{id}/edit
        [Route("{id}/edit")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var champion = _dbContext.Champions.Find(id);
            if (champion == null)
                return NotFound($"Champion with ID {id} not found");

            return View(champion);
        }

        // POST: /api/champions/{id}/edit
        [Route("{id}/edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Champion model)
        {
            var champion = _dbContext.Champions.Find(id);
            if (champion == null)
                return NotFound($"Champion with ID {id} not found");

            if (ModelState.IsValid)
            {
                champion.Name = model.Name;
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(champion);
        }

        // GET: /api/champions/{id}/delete
        [Route("{id}/delete")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var champion = _dbContext.Champions.Include(c => c.Players).FirstOrDefault(c => c.Id == id);
            if (champion == null)
                return NotFound($"Champion with ID {id} not found");

            return View(champion);
        }

        // POST: /api/champions/{id}/delete
        [Route("{id}/delete")]
        [HttpPost]
        public IActionResult DeleteConfirm(int id)
        {
            var champion = _dbContext.Champions.Find(id);
            if (champion == null)
                return NotFound($"Champion with ID {id} not found");

            var playersCount = _dbContext.Players.Count(p => p.ChampionId == id);
            if (playersCount > 0)
            {
                ModelState.AddModelError("", $"Cannot delete champion — {playersCount} player(s) are using it");
                return View("Delete", champion);
            }

            _dbContext.Champions.Remove(champion);
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: /api/champions/from-games
        [Route("from-games")]
        [HttpGet]
        public IActionResult FromGames(string? sort)
        {
            var query = _dbContext.Champions
                .Include(c => c.Players)
                .Where(c => c.Players.Any())
                .AsQueryable();

            query = sort switch
            {
                "appearances_desc" => query.OrderByDescending(c => c.Players.Count).ThenBy(c => c.Name),
                "appearances_asc" => query.OrderBy(c => c.Players.Count).ThenBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                "name_asc" => query.OrderBy(c => c.Name),
                _ => query.OrderBy(c => c.Name)
            };

            ViewBag.CurrentSort = sort ?? "name_asc";
            ViewBag.AppearanceSort = sort == "appearances_desc" ? "appearances_asc" : "appearances_desc";
            ViewBag.NameSort = sort == "name_desc" ? "name_asc" : "name_desc";

            return View(query.ToList());
        }

        // AJAX: /api/champions/search?q=
        [Route("search")]
        [HttpGet]
        public IActionResult Search(string? q)
        {
            var query = _dbContext.Champions.OrderBy(c => c.Name).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var lower = q.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lower));
            }

            var result = query
                .Take(20)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            return Json(result);
        }
    }
}