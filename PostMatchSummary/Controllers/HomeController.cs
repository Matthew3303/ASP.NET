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

    [Route("")]
    [Route("home")]
    [Route("home/index")]
    public IActionResult Index()
    {
        ViewBag.MatchCache = _cacheService;
        return View();
    }

    [Route("home/error")]
    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("about")]
    public IActionResult About()
    {
        ViewBag.Title = "O Aplikaciji";
        return View();
    }
}