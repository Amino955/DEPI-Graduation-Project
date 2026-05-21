using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Services.Interfaces;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : AdminBaseController
{
    private readonly IOrderService _orderService;
    private readonly IDeliveryService _deliveryService;

    public AdminOrdersController(IOrderService orderService, IDeliveryService deliveryService, ApplicationDbContext context) : base(context)
    {
        _orderService = orderService;
        _deliveryService = deliveryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(OrderStatus? status, string? search, int page = 1)
    {
        var orders = await _orderService.GetAllOrdersAsync(status, search, page, 20);
        ViewBag.CurrentStatus = status;
        ViewBag.Search = search;

        var countQuery = _context.Orders.AsQueryable();
        ViewBag.StatusCounts = new
        {
            Pending = await countQuery.CountAsync(o => o.Status == OrderStatus.Pending),
            Confirmed = await countQuery.CountAsync(o => o.Status == OrderStatus.Confirmed),
            Shipped = await countQuery.CountAsync(o => o.Status == OrderStatus.Shipped),
            Delivered = await countQuery.CountAsync(o => o.Status == OrderStatus.Delivered),
            Cancelled = await countQuery.CountAsync(o => o.Status == OrderStatus.Cancelled)
        };

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        ViewBag.DeliveryUsers = await _deliveryService.GetDeliveryUsersAsync();
        return View(order);
    }

    // 1. Confirm order (Pending → Confirmed, then auto Processing)
    [HttpPost("confirm"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmOrder(int orderId, string? notes)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null) return NotFound();
        if (order.Status != OrderStatus.Pending)
        {
            TempData["Error"] = "لا يمكن تأكيد هذا الطلب — الحالة الحالية لا تسمح بذلك";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // Confirm
        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed, notes ?? "تم تأكيد الطلب", adminId);
        // Auto-move to Processing
        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing, "جاري تجهيز الطلب", adminId);

        TempData["Success"] = "✅ تم تأكيد الطلب وبدء التجهيز";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    // 2. Ship order (Processing → Shipped), requires delivery person already assigned
    [HttpPost("ship"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ShipOrder(int orderId, string deliveryUserId, string? notes)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null) return NotFound();
        if (order.Status != OrderStatus.Processing)
        {
            TempData["Error"] = "لا يمكن شحن هذا الطلب — يجب أن يكون في مرحلة التجهيز";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
        if (string.IsNullOrEmpty(deliveryUserId))
        {
            TempData["Error"] = "⚠️ يجب تعيين مندوب توصيل أولاً قبل شحن الطلب";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // Assign delivery person (this also handles re-assignment)
        await _deliveryService.AssignOrderAsync(orderId, deliveryUserId, adminId);
        // Ship
        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Shipped, notes ?? "تم شحن الطلب", adminId);

        TempData["Success"] = "🚚 تم شحن الطلب وإشعار المندوب";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    // 3. Cancel order (allowed for Pending, Confirmed, Processing only)
    [HttpPost("cancel"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId, string? notes)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orderService.GetOrderByIdAsync(orderId);

        if (order == null) return NotFound();

        var cancellableStatuses = new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Processing };
        if (!cancellableStatuses.Contains(order.Status))
        {
            TempData["Error"] = "لا يمكن إلغاء الطلب بعد الشحن";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled, notes ?? "تم الإلغاء من قبل الإدارة", adminId);
        TempData["Error"] = "❌ تم إلغاء الطلب";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignDelivery(int orderId, string deliveryUserId)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _deliveryService.AssignOrderAsync(orderId, deliveryUserId, adminId);
        TempData["Success"] = "تم تعيين المندوب بنجاح";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus, string? note)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null) return NotFound();

        await _orderService.UpdateOrderStatusAsync(orderId, newStatus, note ?? $"تم تغيير الحالة يدوياً إلى {newStatus}", adminId);
        
        TempData["Success"] = "تم تحديث حالة الطلب بنجاح";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}
