using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Cart;
using TellaStore.Models.ViewModels.Orders;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[Authorize]
[Route("orders")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IOrderService orderService, ICartService cartService,
        ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _cartService = cartService;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout()
    {
        if (_cartService.IsEmpty()) return RedirectToAction("Index", "Cart");

        var userId = _userManager.GetUserId(User)!;
        var cartItems = _cartService.GetCart();
        var model = new CheckoutViewModel
        {
            SavedAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && !a.IsDeleted).ToListAsync(),
            Cart = new CartViewModel
            {
                Items = cartItems,
                SubTotal = cartItems.Sum(i => i.UnitPrice * i.Quantity),
                DiscountAmount = cartItems.Sum(i => (i.UnitPrice - i.FinalPrice) * i.Quantity)
            }
        };
        return View(model);
    }
[HttpPost("checkout"), ValidateAntiForgeryToken]
public async Task<IActionResult> Checkout(CheckoutViewModel model)
{
    var userId = _userManager.GetUserId(User)!;

    if (!model.SelectedAddressId.HasValue &&
        (string.IsNullOrWhiteSpace(model.NewStreet) ||
         string.IsNullOrWhiteSpace(model.NewCity) ||
         string.IsNullOrWhiteSpace(model.NewGovernorate)))
    {
        ModelState.AddModelError(string.Empty, "يرجى تحديد عنوان التوصيل");
    }

    if (!ModelState.IsValid)
    {
        model.SavedAddresses = await _context.Addresses
            .Where(a => a.UserId == userId && !a.IsDeleted).ToListAsync();
        model.Cart = new CartViewModel
        {
            Items = _cartService.GetCart(),
            SubTotal = _cartService.GetCart().Sum(i => i.UnitPrice * i.Quantity),
            DiscountAmount = _cartService.GetCart().Sum(i => (i.UnitPrice - i.FinalPrice) * i.Quantity)
        };
        return View(model);
    }

    try
    {
        var order = await _orderService.CreateOrderAsync(model, userId);
        return RedirectToAction(nameof(Confirmation), new { orderId = order.Id });
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError(string.Empty, ex.Message);
        model.SavedAddresses = await _context.Addresses
            .Where(a => a.UserId == userId && !a.IsDeleted).ToListAsync();
        model.Cart = new CartViewModel
        {
            Items = _cartService.GetCart(),
            SubTotal = _cartService.GetCart().Sum(i => i.UnitPrice * i.Quantity),
            DiscountAmount = _cartService.GetCart().Sum(i => (i.UnitPrice - i.FinalPrice) * i.Quantity)
        };
        return View(model);
    }
}
    [HttpGet("confirmation/{orderId}")]
    public async Task<IActionResult> Confirmation(int orderId)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.UserId != userId) return NotFound();
        return View(order);
    }

[HttpGet("myorders")]
public async Task<IActionResult> MyOrders(string? status)
{
    var userId = _userManager.GetUserId(User)!;
    var orders = await _orderService.GetUserOrdersAsync(userId);
    var activeStatus = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();

    ViewBag.StatusCounts = new
    {
        All = orders.Count,
        Pending = orders.Count(o => o.Status == OrderStatus.Pending),
        Active = orders.Count(o => o.Status == OrderStatus.Confirmed
                                || o.Status == OrderStatus.Processing
                                || o.Status == OrderStatus.Shipped),
        Delivered = orders.Count(o => o.Status == OrderStatus.Delivered),
        Cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled
                                   || o.Status == OrderStatus.FailedDelivery)
    };

    orders = activeStatus switch
    {
        "pending"   => orders.Where(o => o.Status == OrderStatus.Pending).ToList(),
        "active"    => orders.Where(o => o.Status == OrderStatus.Confirmed
                                      || o.Status == OrderStatus.Processing
                                      || o.Status == OrderStatus.Shipped).ToList(),
        "delivered" => orders.Where(o => o.Status == OrderStatus.Delivered).ToList(),
        "cancelled" => orders.Where(o => o.Status == OrderStatus.Cancelled
                                      || o.Status == OrderStatus.FailedDelivery).ToList(),
        _           => orders
    };

    ViewBag.ActiveStatus = activeStatus;
    return View(orders);
}

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null || order.UserId != userId) return NotFound();
        return View(order);
    }

    [HttpPost("{id}/cancel"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var userId = _userManager.GetUserId(User)!;
            await _orderService.CancelOrderAsync(id, userId);
            TempData["Success"] = "تم إلغاء الطلب بنجاح";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
