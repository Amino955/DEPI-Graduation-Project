using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TellaStore.Models.Entities;
using TellaStore.Services.Interfaces;

namespace TellaStore.Controllers;

[Authorize]
[Route("notifications")]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 50);
        return View(notifications);
    }

    [HttpPost("markallread"), ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkAllAsReadAsync(userId);
        return Json(new { success = true });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var userId = _userManager.GetUserId(User)!;
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Json(new { count });
    }

    [HttpPost("markread/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkAsReadAsync(id, userId);
        return Json(new { success = true });
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdown()
    {
        var userId = _userManager.GetUserId(User)!;
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 5);
        return PartialView("_NotificationDropdown", notifications);
    }
}
