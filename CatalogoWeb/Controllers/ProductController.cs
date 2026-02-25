using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CatalogoWeb.Data;
using Microsoft.EntityFrameworkCore;
using CatalogoWeb.Models;
using CatalogoWeb.ViewModels;

namespace CatalogoWeb.Controllers;

[Authorize]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None
)]
public class ProductController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    public ProductController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    
    public IActionResult Index(ProductFilterViewModel filters)
    {
        var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();
        

        if (!string.IsNullOrWhiteSpace(filters.Name))
            products = products.Where(p => p.Name.Contains(filters.Name));
        
        if (filters.QuantityFrom.HasValue)
            products = products.Where(p => p.Quantity >= filters.QuantityFrom.Value);

        if (filters.QuantityTo.HasValue)
            products = products.Where(p => p.Quantity <= filters.QuantityTo.Value);


        if (filters.PriceFrom.HasValue)
            products = products.Where(p => p.Price >= filters.PriceFrom.Value);

        if (filters.PriceTo.HasValue)
            products = products.Where(p => p.Price <= filters.PriceTo.Value);
        
        if (filters.DateFrom.HasValue)
        {
            var from = filters.DateFrom.Value.Date;
            products = products.Where(p => p.CreatedAt.Date >= from);
        }

        if (filters.DateTo.HasValue)
        {
            var to = filters.DateTo.Value.Date;
            products = products.Where(p => p.CreatedAt.Date <= to);
        }
        
        if (filters.CategoryIds != null && filters.CategoryIds.Count > 0)
        {
            products = products.Where(p => filters.CategoryIds.Contains(p.CategoryId.Value));
        }

        filters.Categories = _context.Categories.ToList();
        filters.Products = products.ToList();

        return View(filters);
    }
    
    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product product, List<IFormFile> files, int principalIndex)
    {

        ViewBag.Categories = _context.Categories.ToList();
        
        if(files == null || files.Count == 0)
            ModelState.AddModelError("", "Debe agregar minimo una imagen");
        
        foreach(var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLower();
            if(ext != ".jpg" && ext != ".png")
                ModelState.AddModelError("", "Solo se permiten imagenes JPG o PNG");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }
            
        
        product.CreatedAt = DateTime.Now;

        _context.Products.Add(product);
        _context.SaveChanges();
        
        int index = 0;

        foreach(var file in files)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            string path = Path.Combine(_env.WebRootPath, "images", fileName);

            using(var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var image = new Image
            {
                Name = file.FileName,
                FileName = fileName,
                Path = "/images/" + fileName,
                ProductId = product.Id,
                IsPrincipal = false
            };

            _context.Images.Add(image);

            index++;
        }
        _context.SaveChanges();
        
        var images = _context.Images.Where(i=>i.ProductId==product.Id).ToList();
        
        if(images.Count == 1)
        {
            images[0].IsPrincipal = true;
        }
        else
        {
            images[principalIndex].IsPrincipal = true;
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }
    
    public IActionResult Details(int id)
    {
        var product = _context.Products
            .Include(p=>p.Images)
            .Include(p=>p.Category)
            .FirstOrDefault(p=>p.Id==id);

        return View(product);
    }
}