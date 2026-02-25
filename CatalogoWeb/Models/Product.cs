namespace CatalogoWeb.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int? CategoryId { get; set; }
    public Category Category { get; set; }
    public List<Image> Images { get; set; }
}