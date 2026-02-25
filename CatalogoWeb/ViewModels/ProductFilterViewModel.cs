using CatalogoWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace CatalogoWeb.ViewModels;

public class ProductFilterViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? Name { get; set; }
    public int? QuantityFrom { get; set; }
    public int? QuantityTo { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    
    public List<int> CategoryIds { get; set; } = new();
}

