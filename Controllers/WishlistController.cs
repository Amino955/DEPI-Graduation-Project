using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Products;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[Authorize]
[Route("wishlist")]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly IProductService _productService;
    private readonly UserManager<ApplicationUser> _userManager;

    public WishlistController(IWishlistService wishlistService,
        IProductService productService,
        UserManager<ApplicationUser> userManager)
    {
        _wishlistService = wishlistService;
        _productService = productService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var entities = await _wishlistService.GetWishlistAsync(userId);
        
        var cards = new List<ProductCardViewModel>();
        foreach (var p in entities)
        {
            var card = await _productService.ToCardViewModelAsync(p);
            card.IsInWishlist = true;
            cards.Add(card);
        }
        
        return View(cards);
    }


[HttpPost("clear"), ValidateAntiForgeryToken]
public async Task<IActionResult> Clear()
{
    var userId = _userManager.GetUserId(User)!;
    await _wishlistService.ClearWishlistAsync(userId);
    TempData["Success"] = "تم مسح المفضلة بنجاح";
    return RedirectToAction(nameof(Index));
}


    [HttpPost("toggle/{productId}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId)
    {
        var userId = _userManager.GetUserId(User)!;
        var isAdded = await _wishlistService.ToggleAsync(userId, productId);
        return Json(new
        {
            success = true,
            isAdded,
            wishlistCount = await _wishlistService.GetCountAsync(userId)
        });
    }
}
