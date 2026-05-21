using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Services.Interfaces;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : AdminBaseController
{
    private readonly IProductService _productService;

    public AdminProductsController(IProductService productService, ApplicationDbContext context) : base(context)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, bool showInactive = false, int page = 1)
    {
        var products = await _productService.GetProductsAsync(
            categoryId, search, null, null, null, null, null, null, showInactive, "newest", page, 20);
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            "Id", "Name", categoryId
        );
        ViewBag.SearchTerm = search;
        ViewBag.ShowInactive = showInactive;
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            "Id", "Name"
        );
        return View(new ProductFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (model.Variants == null || !model.Variants.Any())
        {
            ModelState.AddModelError("Variants", "يجب إضافة متغير واحد على الأقل للمنتج (مثل المقاس واللون)");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                "Id", "Name", model.CategoryId
            );
            return View(model);
        }
        await _productService.CreateProductAsync(model);
        TempData["Success"] = "تم إضافة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        var model = new ProductFormViewModel
        {
            Id = product.Id, Name = product.Name, Description = product.Description,
            BasePrice = product.BasePrice, CategoryId = product.CategoryId,
            Season = product.Season, IsFeatured = product.IsFeatured, IsActive = product.IsActive,
            Variants = product.Variants.Select(v => new VariantFormViewModel
            {
                Id = v.Id, Color = v.Color, ColorHex = v.ColorHex, Size = v.Size,
                SizeType = v.SizeType, Stock = v.Stock, PriceModifier = v.PriceModifier
            }).ToList()
        };
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            "Id", "Name", product.CategoryId
        );
        ViewBag.ExistingImages = product.Images.OrderBy(i => i.SortOrder).ToList();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
    {
        if (model.Variants == null || !model.Variants.Any())
        {
            ModelState.AddModelError("Variants", "يجب إضافة متغير واحد على الأقل للمنتج (مثل المقاس واللون)");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                "Id", "Name", model.CategoryId
            );
            var product = await _productService.GetProductByIdAsync(id);
            ViewBag.ExistingImages = product?.Images.OrderBy(i => i.SortOrder).ToList() ?? new List<ProductImage>();
            return View(model);
        }
        await _productService.UpdateProductAsync(id, model);
        TempData["Success"] = "تم تحديث المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تعديل حالة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // require explicit confirmation field to avoid accidental deletes from other form posts
        if (!Request.HasFormContentType || Request.Form["confirm"] != "true")
        {
            TempData["Error"] = "لم يتم تأكيد الحذف";
            return RedirectToAction(nameof(Index));
        }
        await _productService.DeleteProductAsync(id);
        TempData["Success"] = "تم حذف المنتج";
        return RedirectToAction(nameof(Index));
    }
}
