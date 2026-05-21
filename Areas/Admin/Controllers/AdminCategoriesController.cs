using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Helpers;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : AdminBaseController
{
    public AdminCategoriesController(ApplicationDbContext context) : base(context) { }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
            .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
            .OrderBy(c => c.SortOrder).ToListAsync();
        return View(categories);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        // Basic validation to avoid inserting null/empty Name which causes SQL errors
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "اسم القسم مطلوب";
            return RedirectToAction(nameof(Index));
        }

        // generate slug and ensure uniqueness for categories
        var baseSlug = string.IsNullOrEmpty(model.Slug) ? SlugHelper.GenerateSlug(model.Name) : model.Slug;
        var slug = baseSlug;
        int suffix = 0;
        while (await _context.Categories.AnyAsync(c => c.Slug == slug))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }
        model.Slug = slug;

        _context.Categories.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إضافة القسم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        var category = await _context.Categories.FindAsync(model.Id);
        if (category == null) return NotFound();
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "اسم القسم مطلوب";
            return RedirectToAction(nameof(Index));
        }
        category.Name = model.Name;
        category.IsActive = model.IsActive;
        category.SortOrder = model.SortOrder;
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تحديث القسم بنجاح";
        return RedirectToAction(nameof(Index));
    }

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    // Get IDs of all subcategories under this parent
    var subCategoryIds = await _context.Categories
        .Where(c => c.ParentCategoryId == id && !c.IsDeleted)
        .Select(c => c.Id)
        .ToListAsync();

    // Check if parent OR any subcategory has active products
    var hasProducts = await _context.Products
        .AnyAsync(p => !p.IsDeleted &&
                       (p.CategoryId == id || subCategoryIds.Contains(p.CategoryId)));

    if (hasProducts)
    {
        TempData["Error"] = "لا يمكن حذف قسم يحتوي على منتجات (بما فيها الأقسام الفرعية)";
        return RedirectToAction(nameof(Index));
    }

    // Soft-delete subcategories first
    if (subCategoryIds.Any())
    {
        var subCategories = await _context.Categories
            .Where(c => subCategoryIds.Contains(c.Id))
            .ToListAsync();
        subCategories.ForEach(c => c.IsDeleted = true);
    }

    // Soft-delete the parent category
    var category = await _context.Categories.FindAsync(id);
    if (category != null)
    {
        category.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    TempData["Success"] = "تم حذف القسم وجميع أقسامه الفرعية";
    return RedirectToAction(nameof(Index));
}

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleActiveRequest req)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        category.IsActive = req?.IsActive ?? category.IsActive;
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    public class ToggleActiveRequest
    {
        public bool IsActive { get; set; }
    }
}
