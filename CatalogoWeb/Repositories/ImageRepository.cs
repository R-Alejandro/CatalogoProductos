using CatalogoWeb.Models;
using CatalogoWeb.Data;
namespace CatalogoWeb.Repositories;

public interface IImageRepository
{
    Task AddRangeAsync(IEnumerable<Image> images);
}

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _context;

    public ImageRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddRangeAsync(IEnumerable<Image> images)
    {
        _context.Images.AddRange(images);
        await _context.SaveChangesAsync();
    }
}