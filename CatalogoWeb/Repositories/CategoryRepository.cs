using CatalogoWeb.Models;
using CatalogoWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Repositories;

public interface ICategoryRepository
{
    IQueryable<Category> GetAll();
    Task<List<Category>> GetAllAsync();
}
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public IQueryable<Category> GetAll()
    {
        var categories = _context.Categories.AsQueryable();
        
        return categories;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        
        return categories;
    }
}