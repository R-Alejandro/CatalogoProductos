namespace CatalogoWeb.Models;
using System.ComponentModel.DataAnnotations;

public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
    public string Name { get; set; }
    
    public List<Product> Products { get; set; } = new();
}