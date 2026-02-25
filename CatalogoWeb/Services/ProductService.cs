using CatalogoWeb.Models;
using CatalogoWeb.ViewModels;
using CatalogoWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Services;

public interface IProductService
{
    List<Product> FilterProducts(ProductFilterViewModel filters);
    List<Category> GetCategories();
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }
    
    public List<Product> FilterProducts(ProductFilterViewModel filters)
    {
        //puede ir para repository? diria que si
        var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable(); 
        
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
        return _context.Categories.ToList();
    }
}