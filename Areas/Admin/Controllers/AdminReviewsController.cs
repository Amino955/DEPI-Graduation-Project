using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController : AdminBaseController
{
    public AdminReviewsController(ApplicationDbContext context) : base(context) { }

    public async Task<IActionResult> Index(bool? approved = null)
    {
        var query = _context.Reviews
            .Include(r => r.User).Include(r => r.Product)
            .AsQueryable();

        if (approved.HasValue)
            query = query.Where(r => r.IsApproved == approved);

        ViewBag.Filter = approved;
        var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return View(reviews);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            review.IsApproved = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "تمت الموافقة على التقييم";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            review.IsDeleted = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف التقييم بنجاح";
        }
        return RedirectToAction(nameof(Index));
    }
}
