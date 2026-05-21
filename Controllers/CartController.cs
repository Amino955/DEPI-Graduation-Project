using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.ViewModels.Cart;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[Route("cart")]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IDiscountService _discountService;
    private readonly ApplicationDbContext _context;

    public CartController(ICartService cartService, IDiscountService discountService,
        ApplicationDbContext context)
    {
        _cartService = cartService;
        _discountService = discountService;
        _context = context;
    }

    [HttpGet("count")]
    public IActionResult Count()
    {
        return Json(new { count = _cartService.GetItemCount() });
    }

    public IActionResult Index()
    {
        var items = _cartService.GetCart();
        var model = new CartViewModel
        {
            Items = items,
            SubTotal = items.Sum(i => i.UnitPrice * i.Quantity),
            DiscountAmount = items.Sum(i => (i.UnitPrice - i.FinalPrice) * i.Quantity)
        };
        return View(model);
    }

    [HttpPost("add"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(v => v.Id == request.VariantId && !v.IsDeleted && v.IsActive);

            if (variant == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

            if (request.Quantity <= 0)
                return Json(new { success = false, message = "يجب إضافة كمية أكبر من صفر" });

            if (variant.Stock < request.Quantity)
                return Json(new { success = false, message = $"الكمية المطلوبة غير متاحة. المتاح: {variant.Stock}" });

            var existingCartItem = _cartService.GetCart()
                .FirstOrDefault(i => i.VariantId == request.VariantId);
            var existingQty = existingCartItem?.Quantity ?? 0;
            var totalRequested = existingQty + request.Quantity;

            if (totalRequested > variant.Stock)
                return Json(new { 
                    success = false, 
                    message = $"الكمية الإجمالية ({totalRequested}) تتجاوز المتاح ({variant.Stock} قطعة). في السلة: {existingQty}"
                });

            var originalPrice = variant.Product.BasePrice + variant.PriceModifier;
            var finalPrice = await _discountService.GetDiscountedPriceAsync(variant.ProductId, originalPrice);
            var mainImage = variant.Product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                ?? variant.Product.Images.FirstOrDefault()?.ImageUrl ?? string.Empty;

            _cartService.AddItem(variant.ProductId, variant.Id, variant.Product.Name,
                variant.Color, variant.ColorHex, variant.Size, originalPrice, finalPrice, request.Quantity, mainImage);

            return Json(new
            {
                success = true,
                message = "تم إضافة المنتج للسلة ✓",
                cartCount = _cartService.GetItemCount()
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
    {
        var variant = await _context.ProductVariants.FindAsync(request.VariantId);
        if (variant != null && request.Quantity > variant.Stock)
            return Json(new { success = false, message = "الكمية المطلوبة غير متاحة" });

        _cartService.UpdateQuantity(request.VariantId, request.Quantity);
        var item = _cartService.GetCart().FirstOrDefault(i => i.VariantId == request.VariantId);
        var items = _cartService.GetCart();
        var subTotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var total = _cartService.GetTotal();
        var discountAmount = subTotal - total;

        return Json(new
        {
            success = true,
            cartCount = _cartService.GetItemCount(),
            cartTotal = _cartService.GetTotal(),
            cartSubTotal = subTotal,
            cartDiscount = discountAmount,
            itemTotal = item?.TotalPrice ?? 0
        });
    }

    [HttpPost("remove/{variantId}"), ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int variantId)
    {
        _cartService.RemoveItem(variantId);
        var items = _cartService.GetCart();
        var subTotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var total = _cartService.GetTotal();
        var discountAmount = subTotal - total;
        return Json(new
        {
            success = true,
            cartCount = _cartService.GetItemCount(),
            cartTotal = total,
            cartSubTotal = subTotal,
            cartDiscount = discountAmount
        });
    }

    [HttpPost("clear"), ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.ClearCart();
        return Json(new { success = true });
    }
}

public record AddToCartRequest(int VariantId, int Quantity);
public record UpdateQuantityRequest(int VariantId, int Quantity);
