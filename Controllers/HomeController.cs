using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Home;
using TellaStore.Models.ViewModels.Products;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly IWishlistService _wishlistService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public HomeController(IProductService productService, 
        IWishlistService wishlistService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _productService = productService;
        _wishlistService = wishlistService;
        _userManager = userManager;
        _context = context;
    }

public async Task<IActionResult> Index()
{
    var featuredEntities = await _productService.GetFeaturedProductsAsync(8);
    var latestEntities = await _productService.GetLatestProductsAsync(8);
    var userId = _userManager.GetUserId(User);

    // Single query to get all wishlist product IDs — eliminates N+1
    var wishlistIds = userId != null
        ? await _wishlistService.GetWishlistProductIdsAsync(userId)
        : new HashSet<int>();

    var featuredCards = new List<ProductCardViewModel>();
    foreach (var p in featuredEntities)
    {
        var card = await _productService.ToCardViewModelAsync(p);
        card.IsInWishlist = wishlistIds.Contains(p.Id);
        featuredCards.Add(card);
    }

    var latestCards = new List<ProductCardViewModel>();
    foreach (var p in latestEntities)
    {
        var card = await _productService.ToCardViewModelAsync(p);
        card.IsInWishlist = wishlistIds.Contains(p.Id);
        latestCards.Add(card);
    }

    var model = new HomeViewModel
    {
        FeaturedProducts = featuredCards,
        LatestProducts = latestCards,
        ParentCategories = await _context.Categories
            .Where(c => c.ParentCategoryId == null && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.SortOrder).ToListAsync(),
        ActiveDiscounts = await _context.Discounts
            .Where(d => d.IsActive && !d.IsDeleted
                && DateTime.UtcNow >= d.StartDate
                && DateTime.UtcNow <= d.EndDate)
            .ToListAsync()
    };
    return View(model);
}
    public IActionResult Error() => View();
}
