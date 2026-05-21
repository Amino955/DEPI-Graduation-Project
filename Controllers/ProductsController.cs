using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Products;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[AllowAnonymous]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IDiscountService _discountService;
    private readonly IWishlistService _wishlistService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductsController(IProductService productService, IDiscountService discountService,
        IWishlistService wishlistService, ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _productService = productService;
        _discountService = discountService;
        _wishlistService = wishlistService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index([FromQuery] ProductFilterViewModel filter)
    {
        if (!string.IsNullOrEmpty(filter.CategorySlug) && !filter.CategoryId.HasValue)
        {
            var cat = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == filter.CategorySlug && !c.IsDeleted);
            if (cat != null) filter.CategoryId = cat.Id;
        }

        var productEntities = await _productService.GetProductsAsync(
            filter.CategoryId, filter.SearchTerm, filter.Color, filter.Size,
            filter.Season, filter.MinPrice, filter.MaxPrice, 
            filter.IsFeatured, false, filter.SortBy, filter.Page, 12);

        var userId = _userManager.GetUserId(User);
        var wishlistIds = userId != null
            ? await _wishlistService.GetWishlistProductIdsAsync(userId)
            : new HashSet<int>();

        var cardList = new List<ProductCardViewModel>();
        foreach (var p in productEntities)
        {
            var card = await _productService.ToCardViewModelAsync(p);
            card.IsInWishlist = wishlistIds.Contains(p.Id);
            cardList.Add(card);
        }

        var pagedCards = new StaticPagedList<ProductCardViewModel>(
            cardList,
            productEntities.PageNumber,
            productEntities.PageSize,
            productEntities.TotalItemCount);

        var model = new ProductListViewModel
        {
            Products = pagedCards,
            Filter = filter,
            Categories = await _context.Categories
                .Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
            AvailableColors = await _productService.GetAvailableColorsAsync(filter.CategoryId),
            AvailableSizes = await _productService.GetAvailableSizesAsync(filter.CategoryId)
        };

        return View(model);
    }

    public async Task<IActionResult> Details(string slug)
    {
        var product = await _productService.GetProductBySlugAsync(slug);
        if (product == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var finalPrice = await _discountService.GetDiscountedPriceAsync(product.Id, product.BasePrice);

        bool canReview = false;
        if (userId != null)
        {
            var hasBought = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == product.Id && oi.Order.UserId == userId
                    && oi.Order.Status == OrderStatus.Delivered);
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == product.Id && r.UserId == userId);
            canReview = hasBought && !hasReviewed;
        }

        var relatedEntities = await _productService.GetRelatedProductsAsync(product.Id, 4);
        var relatedCards = new List<ProductCardViewModel>();
        foreach (var rp in relatedEntities)
        {
            var card = await _productService.ToCardViewModelAsync(rp);
            if (userId != null) card.IsInWishlist = await _wishlistService.IsInWishlistAsync(userId, rp.Id);
            relatedCards.Add(card);
        }

        var model = new ProductDetailsViewModel
        {
            Product = product,
            Variants = product.Variants.Where(v => v.IsActive && !v.IsDeleted).ToList(),
            AvailableColors = product.Variants
                .Where(v => v.IsActive && !v.IsDeleted).Select(v => v.Color).Distinct().ToList(),
            FinalPrice = finalPrice,
            HasDiscount = finalPrice < product.BasePrice,
            OriginalPrice = product.BasePrice,
            Reviews = product.Reviews.Where(r => r.IsApproved).ToList(),
            AverageRating = product.AverageRating,
            IsInWishlist = userId != null && await _wishlistService.IsInWishlistAsync(userId, product.Id),
            CanReview = canReview,
            RelatedProducts = relatedCards
        };

        return View(model);
    }
    [HttpGet("recently-viewed")]
    public async Task<IActionResult> RecentlyViewed([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids)) return Content("");

        var idList = ids.Split(',')
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n > 0)
            .Take(6)
            .ToList();

        var products = await _context.Products
            .Where(p => idList.Contains(p.Id) && p.IsActive && !p.IsDeleted)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .ToListAsync();

        // Preserve original order from localStorage
        products = idList.Select(id => products.FirstOrDefault(p => p.Id == id))
                         .Where(p => p != null).ToList()!;

        var userId = _userManager.GetUserId(User);
        var wishlistIds = userId != null
            ? await _wishlistService.GetWishlistProductIdsAsync(userId)
            : new HashSet<int>();

        var cards = new List<ProductCardViewModel>();
        foreach (var p in products)
        {
            var card = await _productService.ToCardViewModelAsync(p);
            card.IsInWishlist = wishlistIds.Contains(p.Id);
            cards.Add(card);
        }

        return PartialView("_RecentlyViewedCards", cards);
    }
}
