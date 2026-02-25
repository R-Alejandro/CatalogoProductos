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
            .ToList();
        
        if (!string.IsNullOrWhiteSpace(filters.Name))
            products = products.Where(p => p.Name.Contains(filters.Name)).ToList();

        if (filters.QuantityFrom.HasValue)
            products = products.Where(p => p.Quantity >= filters.QuantityFrom.Value).ToList();

        if (filters.QuantityTo.HasValue)
            products = products.Where(p => p.Quantity <= filters.QuantityTo.Value).ToList();

        if (filters.PriceFrom.HasValue)
            products = products.Where(p => p.Price >= filters.PriceFrom.Value).ToList();

        if (filters.PriceTo.HasValue)
            products = products.Where(p => p.Price <= filters.PriceTo.Value).ToList();

        if (filters.DateFrom.HasValue)
            products = products.Where(p => p.CreatedAt.Date >= filters.DateFrom.Value.Date).ToList();

        if (filters.DateTo.HasValue)
            products = products.Where(p => p.CreatedAt.Date <= filters.DateTo.Value.Date).ToList();
        
    
        if (filters.CategoryIds != null && filters.CategoryIds.Any())
        {
            bool includeNull = filters.CategoryIds.Contains(0);
            var selectedIds = filters.CategoryIds.Where(id => id != 0).ToList();

            products = products.Where(p =>
                (p.CategoryId != null && selectedIds.Contains(p.CategoryId.Value)) ||
                (includeNull && p.CategoryId == null)).ToList();
        }

        filters.Products = products;
        filters.Categories = _context.Categories.ToList();

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
        

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(_env.WebRootPath, "images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
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
        }

        _context.SaveChanges();


        var images = _context.Images.Where(i => i.ProductId == product.Id).ToList();
        if (images.Count == 1)
        {
            images[0].IsPrincipal = true;
        }
        else
        {
            if (principalIndex < 0 || principalIndex >= images.Count)
                principalIndex = 0;
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
    
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _context.Products
            .Include(p => p.Images)
            .FirstOrDefault(p => p.Id == id);

        if (product == null) return NotFound();

        var vm = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Quantity = product.Quantity,
            Price = product.Price,
            CategoryId = product.CategoryId,
            ExistingImages = product.Images.ToList()
        };

        ViewBag.Categories = _context.Categories.ToList();
        return View(vm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ProductEditViewModel vm)
    {
        vm.ExistingImages = _context.Images
            .Where(i => i.ProductId == vm.Id)
            .ToList();

        ViewBag.Categories = _context.Categories.ToList();
        
        var idsToDelete = vm.ImagesToDelete ?? new List<int>();
        int surviving = vm.ExistingImages.Count(i => !idsToDelete.Contains(i.Id));
        int newCount   = vm.NewFiles?.Count(f => f.Length > 0) ?? 0;
        int total      = surviving + newCount;

        if (total == 0)
            ModelState.AddModelError("", "El producto debe tener al menos una imagen.");
        
        if (vm.NewFiles != null)
        {
            foreach (var f in vm.NewFiles.Where(f => f.Length > 0))
            {
                var ext = Path.GetExtension(f.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png")
                    ModelState.AddModelError("", $"'{f.FileName}' no es JPG ni PNG.");
            }
        }

        if (!ModelState.IsValid)
            return View(vm);
        
        var product = _context.Products.FirstOrDefault(p => p.Id == vm.Id);
        if (product == null) return NotFound();

        product.Name = vm.Name;
        product.Quantity = vm.Quantity;
        product.Price = vm.Price;
        product.CategoryId = vm.CategoryId;
        
        foreach (var imgId in idsToDelete)
        {
            var img = _context.Images.Find(imgId);
            if (img == null || img.ProductId != vm.Id) continue;


            var fullPath = Path.Combine(_env.WebRootPath, img.FileName.TrimStart('/'));
            var physicalPath = Path.Combine(_env.WebRootPath, "images", img.FileName);
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _context.Images.Remove(img);
        }
        
        var savedNewImages = new List<Image>();
        if (vm.NewFiles != null)
        {
            foreach (var file in vm.NewFiles.Where(f => f.Length > 0))
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string path = Path.Combine(_env.WebRootPath, "images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                    file.CopyTo(stream);

                var img = new Image
                {
                    Name      = file.FileName,
                    FileName  = fileName,
                    Path      = "/images/" + fileName,
                    ProductId = vm.Id,
                    IsPrincipal = false
                };
                _context.Images.Add(img);
                savedNewImages.Add(img);
            }
        }

        _context.SaveChanges(); 
        
        var allImages = _context.Images.Where(i => i.ProductId == vm.Id).ToList();
        allImages.ForEach(i => i.IsPrincipal = false);
        
        Image? principal = null;
        if (!string.IsNullOrEmpty(vm.PrincipalImageKey))
        {
            var parts = vm.PrincipalImageKey.Split('_');
            if (parts.Length == 2)
            {
                if (parts[0] == "existing" && int.TryParse(parts[1], out int existId))
                    principal = allImages.FirstOrDefault(i => i.Id == existId);
                else if (parts[0] == "new" && int.TryParse(parts[1], out int newIdx))
                    principal = savedNewImages.ElementAtOrDefault(newIdx);
            }
        }
        
        principal ??= allImages.FirstOrDefault();
        if (principal != null) principal.IsPrincipal = true;

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

}