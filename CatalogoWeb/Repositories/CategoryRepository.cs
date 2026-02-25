using CatalogoWeb.Models;
using CatalogoWeb.Data;

namespace CatalogoWeb.Repositories;

public interface ICategoryRepository
{
    IQueryable<Category> GetAll();
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
}