using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TellaStore.Models.Entities;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[Authorize(Roles = "Delivery")]
[Route("delivery")]
public class DeliveryController : Controller
{
    private readonly IDeliveryService _deliveryService;
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeliveryController(IDeliveryService deliveryService,
        IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _deliveryService = deliveryService;
        _orderService = orderService;
        _userManager = userManager;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(await _deliveryService.GetAssignedOrdersAsync(userId));
    }

    [HttpGet("orders/{id}")]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();

        // Security: ensure this order is assigned to the current delivery person
        if (order.DeliveryAssignment?.DeliveryUserId != userId)
            return Forbid();

        return View(order);
    }

    [HttpPost("orders/{orderId}/verify"), ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(int orderId, string code)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null) return NotFound();
        if (order.DeliveryAssignment?.DeliveryUserId != userId) return Forbid();

        var success = await _orderService.VerifyDeliveryCodeAsync(orderId, code, userId);

        TempData[success ? "Success" : "Error"] = success
            ? "تم تأكيد التسليم بنجاح ✓"
            : "الكود غير صحيح، يرجى التحقق من العميل مرة أخرى";

        return RedirectToAction(nameof(OrderDetails), new { id = orderId });
    }

    [HttpPost("orders/{orderId}/report-failed"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportFailed(int orderId, string reason)
    {
        var userId = _userManager.GetUserId(User)!;
        
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order?.DeliveryAssignment?.DeliveryUserId != userId)
            return Forbid();

        await _deliveryService.ReportFailedDeliveryAsync(orderId, userId, reason);
        TempData["Warning"] = "تم تسجيل فشل التسليم وإشعار المدير";
        return RedirectToAction(nameof(Index));
    }
}
