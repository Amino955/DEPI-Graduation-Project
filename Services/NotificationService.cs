using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Services.Interfaces;

namespace TellaStore.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task CreateNotificationAsync(string userId, string title,
        string message, NotificationType type, string? link = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link
        });
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10)
        => await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(string userId)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification == null) return;
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        await _context.SaveChangesAsync();
    }

    public async Task NotifyAdminNewOrderAsync(int orderId, string customerName)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        foreach (var admin in admins)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = admin.Id,
                Title = "طلب جديد",
                Message = $"طلب جديد #{orderId} من {customerName}",
                Type = NotificationType.NewOrder,
                Link = $"/admin/orders/{orderId}"
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task NotifyAdminLowStockAsync(int productId, string productName, int stock)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        foreach (var admin in admins)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = admin.Id,
                Title = "⚠️ تحذير: مخزون منخفض",
                Message = $"{productName} — باقي {stock} قطعة فقط",
                Type = NotificationType.LowStock,
                Link = $"/admin/products/edit/{productId}"
            });
        }
        await _context.SaveChangesAsync();
    }
}
