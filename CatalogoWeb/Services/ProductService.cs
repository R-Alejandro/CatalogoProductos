using CatalogoWeb.Models;
using CatalogoWeb.ViewModels;
using CatalogoWeb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Services;

public interface IProductService
{
    Task<List<Product>> FilterProductsAsync(ProductFilterViewModel filters);
    List<Category> GetCategories();
    Task<ServiceResult> CreateAsync(Product product, List<IFormFile> files, int principalIndex);
    Task<Product?> GetProductDetailsAsync(int id);
    Task<ProductEditViewModel?> GetEditViewModelAsync(int id);
    Task<List<Category>> GetCategoriesAsync();
    Task<ServiceResult> UpdateAsync(ProductEditViewModel vm);
    Task<List<Image>> GetImagesAsync(int productId);
}

//contenedor para obtener errores, tal vez una Task???? pero es muy
public class ServiceResult
{
    public bool Success => !Errors.Any();
    public List<string> Errors { get; set; } = new();
}
public class ProductService : IProductService
{
    private readonly IWebHostEnvironment _env;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IImageRepository _imageRepository;

    public ProductService(
        IProductRepository repository, 
        ICategoryRepository categoryRepository,
        IImageRepository imageRepository,
        IWebHostEnvironment env
        )
    {
        _productRepository = repository;
        _categoryRepository = categoryRepository;
        _imageRepository = imageRepository;
        _env = env;
    }
    
    public async Task<List<Product>> FilterProductsAsync(ProductFilterViewModel filters)
    {

        var products = _productRepository.GetAll();
        
        if (!string.IsNullOrWhiteSpace(filters.Name))
            products = products.Where(p => p.Name.ToLower().Contains(filters.Name.ToLower()));

        if (filters.QuantityFrom.HasValue)
            products = products.Where(p => p.Quantity >= filters.QuantityFrom.Value);

        if (filters.QuantityTo.HasValue)
            products = products.Where(p => p.Quantity <= filters.QuantityTo.Value);

        if (filters.PriceFrom.HasValue)
            products = products.Where(p => p.Price >= filters.PriceFrom.Value);

        if (filters.PriceTo.HasValue)
            products = products.Where(p => p.Price <= filters.PriceTo.Value);

        if (filters.DateFrom.HasValue)
            products = products.Where(p => p.CreatedAt.Date >= filters.DateFrom.Value.Date);

        if (filters.DateTo.HasValue)
            products = products.Where(p => p.CreatedAt.Date <= filters.DateTo.Value.Date);
        
        if (filters.CategoryIds != null && filters.CategoryIds.Any())
        {
            bool includeNull = filters.CategoryIds.Contains(0);
            var selectedIds = filters.CategoryIds.Where(id => id != 0).ToList();

            products = products.Where(p =>
                (p.CategoryId != null && selectedIds.Contains(p.CategoryId.Value)) ||
                (includeNull && p.CategoryId == null));
        }
        
        return await products.ToListAsync();
    }

    public List<Category> GetCategories()
    {
        return _categoryRepository.GetAll().ToList();
    }
    
    public async Task<ServiceResult> CreateAsync(Product product, List<IFormFile> files, int principalIndex)
    {
        var result = new ServiceResult();

        if (files == null || files.Count == 0)
            result.Errors.Add("Debe agregar minimo una imagen");

        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLower();
            if (ext != ".jpg" && ext != ".png")
                result.Errors.Add("Solo se permiten imagenes JPG o PNG");
        }

        if (!result.Success)
            return result;

        product.CreatedAt = DateTime.Now;
        
        await _productRepository.AddAsync(product);

        var images = new List<Image>();

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(_env.WebRootPath, "images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            images.Add(new Image
            {
                Name = file.FileName,
                FileName = fileName,
                Path = "/images/" + fileName,
                ProductId = product.Id,
                IsPrincipal = false
            });
        }

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
        
        await _imageRepository.AddRangeAsync(images);
        return result;
    }
    
    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }
    
    public async Task<ProductEditViewModel?> GetEditViewModelAsync(int id)
    {
        var product = await _productRepository.GetWithImagesAsync(id);

        if (product == null)
            return null;
        
        ProductEditViewModel productEditViewModel = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Quantity = product.Quantity,
            Price = product.Price,
            CategoryId = product.CategoryId,
            ExistingImages = product.Images.ToList()
        };
        
        return productEditViewModel;
    }
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }
    
    public async Task<ServiceResult> UpdateAsync(ProductEditViewModel vm)
    {
        var result = new ServiceResult();
        var product = await _productRepository.GetWithImagesAsync(vm.Id);
        if (product == null)
        {
            result.Errors.Add("Producto no encontrado");
            return result;
        }

        var idsToDelete = vm.ImagesToDelete ?? new List<int>();
        var surviving = product.Images.Count(i => !idsToDelete.Contains(i.Id));
        var newCount = vm.NewFiles?.Count(f => f.Length > 0) ?? 0;
        var total = surviving + newCount;

        if (total == 0)
            result.Errors.Add("El producto debe tener al menos una imagen");

        if (vm.NewFiles != null)
        {
            foreach (var f in vm.NewFiles.Where(f => f.Length > 0))
            {
                var ext = Path.GetExtension(f.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png")
                    result.Errors.Add($"'{f.FileName}' no es JPG ni PNG");
            }
        }

        if (!result.Success)
            return result;
        
        product.Name = vm.Name;
        product.Quantity = vm.Quantity;
        product.Price = vm.Price;
        product.CategoryId = vm.CategoryId;
        
        foreach (var imgId in idsToDelete)
        {
            var img = product.Images.FirstOrDefault(i => i.Id == imgId);
            if (img == null) continue;
            
            var physicalPath = Path.Combine(_env.WebRootPath, "images", img.FileName);
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            await _imageRepository.DeleteAsync(img);
        }
        
        var savedNewImages = new List<Image>();
        if (vm.NewFiles != null)
        {
            foreach (var file in vm.NewFiles.Where(f => f.Length > 0))
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string path = Path.Combine(_env.WebRootPath, "images", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                    await file.CopyToAsync(stream);

                var img = new Image
                {
                    Name = file.FileName,
                    FileName = fileName,
                    Path = "/images/" + fileName,
                    ProductId = vm.Id,
                    IsPrincipal = false
                };

                await _imageRepository.AddAsync(img);
                savedNewImages.Add(img);
            }
        }
        
        var allImages = await _imageRepository.GetByProductIdAsync(vm.Id);
        foreach (var img in allImages)
            img.IsPrincipal = false;

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
        if (principal != null)
            principal.IsPrincipal = true;
        await _productRepository.SaveChangesAsync();
        return result;
    }
    public async Task<List<Image>> GetImagesAsync(int productId)
    {
        return await _imageRepository.GetByProductIdAsync(productId);
    }
}