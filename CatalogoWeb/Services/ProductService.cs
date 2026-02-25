using CatalogoWeb.Models;
using CatalogoWeb.ViewModels;
using CatalogoWeb.Repositories;

namespace CatalogoWeb.Services;

public interface IProductService
{
    List<Product> FilterProducts(ProductFilterViewModel filters);
    List<Category> GetCategories();
    Task<ServiceResult> CreateAsync(Product product, List<IFormFile> files, int principalIndex);
    Task<Product?> GetProductDetailsAsync(int id);
    Task<ProductEditViewModel?> GetEditViewModelAsync(int id);
    Task<List<Category>> GetCategoriesAsync();
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
    
    public List<Product> FilterProducts(ProductFilterViewModel filters)
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
        
        return products.ToList();
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
}