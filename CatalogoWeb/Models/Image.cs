namespace CatalogoWeb.Models;

public class Image
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ProductId { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
    public bool IsPrincipal { get; set; }
    public Product Product { get; set; }
}