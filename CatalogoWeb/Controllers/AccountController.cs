using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using CatalogoWeb.Data;
using CatalogoWeb.Models;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    public AccountController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(string name, string username, string password)
    {
        if (_context.Users.Any(u => u.Username == username))
        {
            ViewBag.Error = "Usuario ya existe";
            return View();
        }
        
        var hasher = new PasswordHasher<User>();
        
        var user = new User()
        {
            Name = name,
            Username = username,
            PasswordHash = hasher.HashPassword(null, password),
            RoleId = 2 //en la siguiente version se deberia eliminar este numero magico
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Login");
    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }
}