using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CatalogoWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoWeb.Controllers;

[Authorize]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None
)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Product");
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
}