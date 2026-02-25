using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using CatalogoWeb.Data;
using CatalogoWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AppDbContext _context;
    public AccountController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            ViewBag.Error = "Usuario no encontrado";
            return View();
        }
        
        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
        );
        
        if (result == PasswordVerificationResult.Failed)
        {
            ViewBag.Error = "Password incorrecto";
            return View();
        }
        
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.Role,user.Rol.Name)
        };
        
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(principal);
        return RedirectToAction("Index", "Product");
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction(
                "Index",
                "Product"
            );
        }
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