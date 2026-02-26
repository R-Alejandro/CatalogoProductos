using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CatalogoWeb.Data;
using Microsoft.EntityFrameworkCore;
using CatalogoWeb.Models;
using CatalogoWeb.Services;
using CatalogoWeb.ViewModels;

namespace CatalogoWeb.Controllers;

[Authorize]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None
)]
public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {

        _productService = productService;
    }
    
    public async Task<IActionResult> Index(ProductFilterViewModel filters)
    {
        filters.Products = await _productService.FilterProductsAsync(filters);
        filters.Categories = await _productService.GetCategoriesAsync();

        return View(filters);
    }
    
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, List<IFormFile> files, int principalIndex)
    {
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        
        var result = await _productService.CreateAsync(product, files, principalIndex);
        //el contenedor
        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);

            return View(product);
        }
        
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetProductDetailsAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var productViewModel = await _productService.GetEditViewModelAsync(id);
        if (productViewModel == null)
            return NotFound();
        
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        return View(productViewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductEditViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.ExistingImages = await _productService.GetImagesAsync(viewModel.Id);
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(viewModel);
        }

        var result = await _productService.UpdateAsync(viewModel);

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);

            viewModel.ExistingImages = await _productService.GetImagesAsync(viewModel.Id);
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(viewModel);
        }

        return RedirectToAction("Index");
    }

}