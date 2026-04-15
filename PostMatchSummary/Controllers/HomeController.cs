using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PostMatchSummary.Models;
using PostMatchSummary.Services;

namespace PostMatchSummary.Controllers;

public class HomeController : Controller
{
    private readonly MatchCacheService _cacheService;

    public HomeController(MatchCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public IActionResult Index()
    {
        // Proslijedi MatchCacheService u view kroz model ili ViewBag
        ViewBag.MatchCache = _cacheService;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
