using TellaStore.Models.Entities;

namespace TellaStore.Services.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message,
                                  NotificationType type, string? link = null);
    Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task NotifyAdminNewOrderAsync(int orderId, string customerName);
    Task NotifyAdminLowStockAsync(int productId, string productName, int stock);
}
