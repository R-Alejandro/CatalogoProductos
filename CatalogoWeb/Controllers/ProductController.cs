using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogoWeb.Controllers;

[Authorize]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None
)]
public class ProductController : Controller
{

    public IActionResult Index()
    {
        return View();
    }
}