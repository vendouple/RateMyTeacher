using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Models;

namespace RateMyTeacher.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index), "Home") });
        }

        var dashboardResult = RedirectToPrimaryDashboard();
        if (dashboardResult is not null)
        {
            return dashboardResult;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private IActionResult? RedirectToPrimaryDashboard()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Admin", "Dashboard");
        }

        if (User.IsInRole("Teacher"))
        {
            return RedirectToAction("Teacher", "Dashboard");
        }

        if (User.IsInRole("Student"))
        {
            return RedirectToAction("Student", "Dashboard");
        }

        return null;
    }
}
