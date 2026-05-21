using TellaStore.Models.Entities;

namespace TellaStore.Services.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryAssignment> AssignOrderAsync(int orderId, string deliveryUserId, string adminId);
    Task<bool> ReportFailedDeliveryAsync(int orderId, string deliveryUserId, string reason);
    Task<List<Order>> GetAssignedOrdersAsync(string deliveryUserId);
    Task<List<ApplicationUser>> GetDeliveryUsersAsync();
}
