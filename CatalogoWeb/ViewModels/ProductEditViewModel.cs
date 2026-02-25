using System.ComponentModel.DataAnnotations;
using CatalogoWeb.Models;
namespace CatalogoWeb.ViewModels;


public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
    public string Name { get; set; }
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }
    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }
    [Required(ErrorMessage = "Debe seleccionar una categoria")]
    public int? CategoryId { get; set; }
    public List<Image> ExistingImages { get; set; } = new();
    public List<int> ImagesToDelete { get; set; } = new();
    public List<IFormFile>? NewFiles { get; set; }
    public string? PrincipalImageKey { get; set; }
}