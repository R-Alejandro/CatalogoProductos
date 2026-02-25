using CatalogoWeb.Models;
using CatalogoWeb.Data;
using Microsoft.EntityFrameworkCore;
namespace CatalogoWeb.Repositories;

public interface IImageRepository
{
    Task AddRangeAsync(IEnumerable<Image> images);
    Task DeleteAsync(Image img);
    Task AddAsync(Image image);
    Task<List<Image>> GetByProductIdAsync(int productId);
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
    public async Task AddAsync(Image image)
    {
        await _context.Images.AddAsync(image);
    }

    public Task DeleteAsync(Image img)
    {
        _context.Images.Remove(img);
        return Task.CompletedTask;
    }

    public async Task<List<Image>> GetByProductIdAsync(int productId)
    {
        return await _context.Images
            .Where(i => i.ProductId == productId)
            .ToListAsync();
    }

}