using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;

namespace TellaStore.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int productId, int rating, string? comment)
    {
        var userId = _userManager.GetUserId(User)!;

        // Verify purchase
        var hasBought = await _context.OrderItems
            .AnyAsync(oi => oi.ProductId == productId
                && oi.Order.UserId == userId
                && oi.Order.Status == OrderStatus.Delivered);

        if (!hasBought)
        {
            TempData["Error"] = "يمكنك التقييم فقط بعد استلام المنتج";
            return RedirectToAction("Details", "Products",
                new { slug = await GetSlugAsync(productId) });
        }

        // Check not already reviewed
        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

        if (alreadyReviewed)
        {
            TempData["Error"] = "لقد قمت بتقييم هذا المنتج من قبل";
            return RedirectToAction("Details", "Products",
                new { slug = await GetSlugAsync(productId) });
        }

        _context.Reviews.Add(new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment,
            IsVerifiedPurchase = true,
            IsApproved = false
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "شكراً! تم استلام تقييمك وهو قيد المراجعة حالياً";
        return RedirectToAction("Details", "Products",
            new { slug = await GetSlugAsync(productId) });
    }

    private async Task<string> GetSlugAsync(int productId)
    {
        return await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => p.Slug)
            .FirstOrDefaultAsync() ?? string.Empty;
    }
}
