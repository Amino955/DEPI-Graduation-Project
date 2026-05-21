using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Services.Interfaces;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminDiscountsController : AdminBaseController
{
    private readonly IDiscountService _discountService;

    public AdminDiscountsController(IDiscountService discountService, ApplicationDbContext context) : base(context)
    {
        _discountService = discountService;
    }

    public async Task<IActionResult> Index() => View(await _discountService.GetAllDiscountsAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new DiscountFormViewModel
        {
            Products = await _context.Products.Where(p => !p.IsDeleted && p.IsActive).ToListAsync(),
            Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).ToListAsync()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiscountFormViewModel model)
    {
        ValidateDiscountModel(model);

        if (!ModelState.IsValid)
        {
            model.Products = await _context.Products.Where(p => !p.IsDeleted && p.IsActive).ToListAsync();
            model.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).ToListAsync();
            return View(model);
        }

        await _discountService.CreateDiscountAsync(model);
        TempData["Success"] = "تم إضافة الخصم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var discount = await _discountService.GetDiscountByIdAsync(id);
        if (discount == null) return NotFound();

        var model = new DiscountFormViewModel
        {
            Id = discount.Id,
            Name = discount.Name,
            Target = discount.Target,
            Type = discount.Type,
            Value = discount.Value,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            IsActive = discount.IsActive,
            ProductId = discount.ProductId,
            CategoryId = discount.CategoryId,
            Products = await _context.Products.Where(p => !p.IsDeleted && p.IsActive).ToListAsync(),
            Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).ToListAsync()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DiscountFormViewModel model)
    {
        ValidateDiscountModel(model);

        if (!ModelState.IsValid)
        {
            model.Products = await _context.Products.Where(p => !p.IsDeleted && p.IsActive).ToListAsync();
            model.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsActive).ToListAsync();
            return View(model);
        }

        await _discountService.UpdateDiscountAsync(id, model);
        TempData["Success"] = "تم تحديث الخصم بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleActiveRequest req)
    {
        var discount = await _context.Discounts.FindAsync(id);
        if (discount == null) return NotFound();
        discount.IsActive = req?.IsActive ?? !discount.IsActive;
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    public class ToggleActiveRequest
    {
        public bool IsActive { get; set; }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!Request.HasFormContentType || Request.Form["confirm"] != "true")
        {
            TempData["Error"] = "لم يتم تأكيد الحذف";
            return RedirectToAction(nameof(Index));
        }
        await _discountService.DeleteDiscountAsync(id);
        TempData["Success"] = "تم حذف الخصم";
        return RedirectToAction(nameof(Index));
    }
    private void ValidateDiscountModel(DiscountFormViewModel model)
    {
        if (!model.StartDate.HasValue)
            ModelState.AddModelError("StartDate", "تاريخ البدء مطلوب");
        if (!model.EndDate.HasValue)
            ModelState.AddModelError("EndDate", "تاريخ الانتهاء مطلوب");

        if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate <= model.StartDate)
            ModelState.AddModelError("EndDate", "تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية");

        if (model.Target == DiscountTarget.Product && !model.ProductId.HasValue)
            ModelState.AddModelError("ProductId", "يجب اختيار منتج للخصم");

        if (model.Target == DiscountTarget.Category && !model.CategoryId.HasValue)
            ModelState.AddModelError("CategoryId", "يجب اختيار قسم للخصم");

        if (model.Type == DiscountType.Percentage && (model.Value < 0 || model.Value > 100))
            ModelState.AddModelError("Value", "نسبة الخصم يجب أن تكون بين 0 و 100");
    }
}
