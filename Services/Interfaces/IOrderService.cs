using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Models.ViewModels.Orders;
using X.PagedList;

namespace TellaStore.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CheckoutViewModel model, string userId);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<List<Order>> GetUserOrdersAsync(string userId);
    Task<IPagedList<Order>> GetAllOrdersAsync(OrderStatus? status, string? search, int page, int pageSize);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes, string adminId);
    Task<bool> VerifyDeliveryCodeAsync(int orderId, string code, string deliveryUserId);
    Task CancelOrderAsync(int orderId, string userId);
    Task<DashboardViewModel> GetDashboardDataAsync();
}
