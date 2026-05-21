using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Services.Interfaces;

namespace TellaStore.Services;

public class DeliveryService : IDeliveryService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public DeliveryService(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task<DeliveryAssignment> AssignOrderAsync(int orderId,
        string deliveryUserId, string adminId)
    {
        var order = await _context.Orders
            .Include(o => o.DeliveryAssignment)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new Exception("الطلب غير موجود");

        if (order.Status != OrderStatus.Processing && order.Status != OrderStatus.Shipped)
            throw new InvalidOperationException("يمكن تعيين المندوب في مرحلة التجهيز أو الشحن فقط");

        DeliveryAssignment assignment;
        if (order.DeliveryAssignment != null)
        {
            assignment = order.DeliveryAssignment;
            assignment.DeliveryUserId = deliveryUserId;
            assignment.Status = DeliveryAssignmentStatus.Assigned;
            assignment.AssignedAt = DateTime.UtcNow;
            assignment.DeliveredAt = null;
            assignment.Notes = null;
            assignment.ProofImageUrl = null;
            assignment.FailureReason = null;
        }
        else
        {
            assignment = new DeliveryAssignment
            {
                OrderId = orderId,
                DeliveryUserId = deliveryUserId,
                Status = DeliveryAssignmentStatus.Assigned
            };
            _context.DeliveryAssignments.Add(assignment);
        }
        await _context.SaveChangesAsync();

        var notifMessage = order.Status == OrderStatus.Shipped
            ? $"تم تعيينك لتسليم الطلب #{orderId} — جاهز للتسليم الآن"
            : $"تم تعيينك لتسليم الطلب #{orderId} — سيتم إخطارك عند الشحن";

        await _notificationService.CreateNotificationAsync(
            deliveryUserId,
            "📦 طلب تسليم جديد",
            notifMessage,
            NotificationType.OrderShipped,
            $"/delivery/orders/{orderId}");

        return assignment;
    }

    public async Task<bool> ReportFailedDeliveryAsync(int orderId,
        string deliveryUserId, string reason)
    {
        var assignment = await _context.DeliveryAssignments
            .FirstOrDefaultAsync(a => a.OrderId == orderId && a.DeliveryUserId == deliveryUserId);

        if (assignment == null) return false;

        assignment.Status = DeliveryAssignmentStatus.Failed;
        assignment.FailureReason = reason;

        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            var oldStatus = order.Status;
            order.Status = OrderStatus.FailedDelivery;
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = OrderStatus.FailedDelivery,
                Notes = $"فشل التسليم — السبب: {reason}",
                ChangedByUserId = deliveryUserId
            });
        }

        await _context.SaveChangesAsync();

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        foreach (var admin in admins)
            await _notificationService.CreateNotificationAsync(
                admin.Id,
                "⚠️ فشل التسليم",
                $"فشل تسليم الطلب #{orderId} — السبب: {reason}",
                NotificationType.NewOrder,
                $"/admin/orders/{orderId}");

        return true;
    }

    public async Task<List<Order>> GetAssignedOrdersAsync(string deliveryUserId)
        => await _context.DeliveryAssignments
            .Where(a => a.DeliveryUserId == deliveryUserId
                && a.Status != DeliveryAssignmentStatus.Delivered)
            .Include(a => a.Order).ThenInclude(o => o.Items)
            .Include(a => a.Order).ThenInclude(o => o.User)
            .Select(a => a.Order)
            .ToListAsync();

    public async Task<List<ApplicationUser>> GetDeliveryUsersAsync()
        => (await _userManager.GetUsersInRoleAsync("Delivery")).ToList();
}
