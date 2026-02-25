using CatalogoWeb.Models;
using CatalogoWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Repositories;

public interface IProductRepository
{
    IQueryable<Product> GetAll();
    Task AddAsync(Product product);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetWithImagesAsync(int id);
}
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public IQueryable<Product> GetAll()
    {
        var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable(); 
        
        return products;
    }
    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p=>p.Images)
            .Include(p=>p.Category)
            .FirstOrDefaultAsync(p=>p.Id==id);
        return product;
    }
    
    public async Task<Product?> GetWithImagesAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        return product;
    }
}