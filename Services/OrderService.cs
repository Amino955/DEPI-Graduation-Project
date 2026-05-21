using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X.PagedList;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Models.ViewModels.Orders;
using TellaStore.Services.Interfaces;
using TellaStore.Settings;

namespace TellaStore.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;
    private readonly IDiscountService _discountService;
    private readonly INotificationService _notificationService;
    private readonly AppSettings _settings;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderService(ApplicationDbContext context, ICartService cartService,
        IDiscountService discountService, INotificationService notificationService,
        IOptions<AppSettings> settings, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _cartService = cartService;
        _discountService = discountService;
        _notificationService = notificationService;
        _settings = settings.Value;
        _userManager = userManager;
    }

    public async Task<Order> CreateOrderAsync(CheckoutViewModel model, string userId)
    {
        var cartItems = _cartService.GetCart();
        if (!cartItems.Any())
            throw new InvalidOperationException("سلة التسوق فارغة");

        // Step 1: Verify stock for ALL items before doing anything
        foreach (var item in cartItems)
        {
            var variant = await _context.ProductVariants.FindAsync(item.VariantId)
                ?? throw new Exception($"المنتج غير موجود: {item.VariantId}");
            if (variant.Stock < item.Quantity)
                throw new InvalidOperationException(
                    $"الكمية المطلوبة من {item.ProductName} ({item.Color} - {item.Size}) غير متاحة. المتاح: {variant.Stock}");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            // Step 2: Resolve shipping address
            string street, city, governorate;
            if (model.SelectedAddressId.HasValue)
            {
                var address = await _context.Addresses.FindAsync(model.SelectedAddressId)
                    ?? throw new Exception("العنوان غير موجود");
                street = address.Street;
                city = address.City;
                governorate = address.Governorate;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.NewStreet) ||
                    string.IsNullOrWhiteSpace(model.NewCity) ||
                    string.IsNullOrWhiteSpace(model.NewGovernorate))
                    throw new InvalidOperationException("يرجى إدخال عنوان التوصيل كاملاً");

                street = model.NewStreet!;
                city = model.NewCity!;
                governorate = model.NewGovernorate!;

                if (model.SaveNewAddress)
                {
                    var existingCount = await _context.Addresses
                        .CountAsync(a => a.UserId == userId && !a.IsDeleted);
                    if (existingCount < _settings.MaxAddressesPerUser)
                        _context.Addresses.Add(new Address
                        {
                            UserId = userId,
                            Label = "عنوان جديد",
                            Street = street,
                            City = city,
                            Governorate = governorate
                        });
                }
            }

            // Step 3: Calculate totals with discounts
            decimal subTotal = 0;
            decimal discountAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in cartItems)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product).ThenInclude(p => p.Images)
                    .FirstAsync(v => v.Id == item.VariantId);

                var originalPrice = variant.Product.BasePrice + variant.PriceModifier;
                var finalPrice = await _discountService.GetDiscountedPriceAsync(
                    variant.ProductId, originalPrice);

                subTotal += originalPrice * item.Quantity;
                discountAmount += (originalPrice - finalPrice) * item.Quantity;

                var mainImage = variant.Product.Images
                    .FirstOrDefault(i => i.IsMain)?.ImageUrl
                    ?? variant.Product.Images.FirstOrDefault()?.ImageUrl
                    ?? string.Empty;

                orderItems.Add(new OrderItem
                {
                    ProductId = variant.ProductId,
                    VariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    Color = variant.Color,
                    Size = variant.Size,
                    ImageUrl = mainImage,
                    UnitPrice = finalPrice,
                    Quantity = item.Quantity
                });
            }

            // Step 4: Create the order
            var order = new Order
            {
                UserId = userId,
                ShippingStreet = street,
                ShippingCity = city,
                ShippingGovernorate = governorate,
                ShippingNotes = model.Notes,
                CustomerPhone = model.Phone,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                Total = subTotal - discountAmount,
                Status = OrderStatus.Pending,
                Items = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Step 5: Deduct stock and check for low stock
            foreach (var item in cartItems)
            {
                var variant = await _context.ProductVariants.FindAsync(item.VariantId)!;
                variant!.Stock -= item.Quantity;

                if (variant.Stock <= _settings.LowStockThreshold)
                {
                    var product = await _context.Products.FindAsync(variant.ProductId)!;
                    if (product != null)
                        await _notificationService.NotifyAdminLowStockAsync(
                            product.Id,
                            $"{product.Name} ({variant.Color} - {variant.Size})",
                            variant.Stock);
                }
            }

            // Step 6: Add initial status history
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = OrderStatus.Pending,
                NewStatus = OrderStatus.Pending,
                Notes = "تم إنشاء الطلب",
                ChangedByUserId = userId
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Step 7: Clear cart and notify admin ONLY after successful commit
            _cartService.ClearCart();
            await _notificationService.NotifyAdminNewOrderAsync(order.Id, user?.FullName ?? "عميل");

            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus,
        string? notes, string adminId)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new Exception("الطلب غير موجود");

        var oldStatus = order.Status;
        order.Status = newStatus;

        // Generate 4-digit delivery code when order is shipped
        if (newStatus == OrderStatus.Shipped && string.IsNullOrEmpty(order.DeliveryCode))
            order.DeliveryCode = RandomNumberGenerator.GetInt32(1000, 10000).ToString();

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Notes = notes,
            ChangedByUserId = adminId
        });

  // Restore stock if cancelled by admin (for any status except already Delivered or already Cancelled)
if (newStatus == OrderStatus.Cancelled &&
    oldStatus != OrderStatus.Delivered &&
    oldStatus != OrderStatus.Cancelled)
{
    var orderWithItems = await _context.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId);

    if (orderWithItems != null)
    {
        foreach (var item in orderWithItems.Items)
        {
            var variant = await _context.ProductVariants.FindAsync(item.VariantId);
            if (variant != null) variant.Stock += item.Quantity;
        }
    }
}
// Deduct stock if reactivated from Cancelled to an active status
else if (oldStatus == OrderStatus.Cancelled &&
         newStatus != OrderStatus.Cancelled &&
         newStatus != OrderStatus.FailedDelivery)
{
    var orderWithItems = await _context.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId);

    if (orderWithItems != null)
    {
        foreach (var item in orderWithItems.Items)
        {
            var variant = await _context.ProductVariants.FindAsync(item.VariantId)
                ?? throw new Exception($"المنتج الفرعي غير موجود: {item.VariantId}");

            if (variant.Stock < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"لا يمكن استعادة الطلب. الكمية المطلوبة من {item.ProductName} ({variant.Color} - {variant.Size}) غير متوفرة في المخزن. المتاح حالياً: {variant.Stock}");
            }
            variant.Stock -= item.Quantity;
        }
    }
}

        // Notify customer based on status
        var (title, message) = newStatus switch
        {
            OrderStatus.Confirmed => ("✅ تم تأكيد طلبك", $"تم تأكيد طلبك #{orderId}"),
            OrderStatus.Processing => ("⚙️ جاري تجهيز طلبك", $"طلبك #{orderId} قيد التجهيز الآن"),
            OrderStatus.Shipped => ("🚚 طلبك في الطريق", $"طلبك #{orderId} في الطريق — كود الاستلام: {order.DeliveryCode}"),
            OrderStatus.Delivered => ("✓ تم تسليم طلبك", $"تم تسليم طلبك #{orderId} بنجاح"),
            OrderStatus.Cancelled => ("❌ تم إلغاء طلبك من قبل الإدارة", $"تم إلغاء طلبك #{orderId} من قبل الإدارة. إذا دفعت مبلغاً، سيتم استرداده."),
            OrderStatus.FailedDelivery => ("فشل توصيل طلبك", $"لم يتمكن المندوب من توصيل طلبك #{orderId}، سيتواصل معك المتجر قريباً"),
            _ => ("تحديث الطلب", $"تم تحديث حالة طلبك #{orderId}")
        };

var notifType = newStatus switch
{
    OrderStatus.Confirmed    => NotificationType.OrderConfirmed,
    OrderStatus.Processing   => NotificationType.OrderProcessing,
    OrderStatus.Shipped      => NotificationType.OrderShipped,
    OrderStatus.Delivered    => NotificationType.OrderDelivered,
    OrderStatus.Cancelled    => NotificationType.OrderCancelled,
    _                        => NotificationType.OrderConfirmed
};

        await _notificationService.CreateNotificationAsync(
            order.UserId, title, message, notifType, $"/orders/{orderId}");

        await _context.SaveChangesAsync();
    }

    public async Task<bool> VerifyDeliveryCodeAsync(int orderId, string code, string deliveryUserId)
    {
        var order = await _context.Orders
            .Include(o => o.DeliveryAssignment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.DeliveryCode != code) return false;

        if (order.Status != OrderStatus.Shipped)
            return false; // لا يمكن تأكيد التسليم إلا إذا كان الأوردر في مرحلة الشحن

        if (order.DeliveryAssignment == null)
            return false;

        if (order.DeliveryAssignment.DeliveryUserId != deliveryUserId)
            return false;

        order.IsCodeVerified = true;
        order.Status = OrderStatus.Delivered;

        if (order.DeliveryAssignment != null)
        {
            order.DeliveryAssignment.Status = DeliveryAssignmentStatus.Delivered;
            order.DeliveryAssignment.DeliveredAt = DateTime.UtcNow;
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = OrderStatus.Shipped,
            NewStatus = OrderStatus.Delivered,
            Notes = "تم التسليم وتأكيد الكود بنجاح",
            ChangedByUserId = deliveryUserId
        });

        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            order.UserId, "✓ تم تسليم طلبك",
            $"تم تسليم طلبك #{orderId} بنجاح",
            NotificationType.OrderDelivered, $"/orders/{orderId}");

        return true;
    }

    public async Task CancelOrderAsync(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new Exception("الطلب غير موجود");

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("لا يمكن إلغاء الطلب بعد أن بدأ التجهيز");

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;

        // Restore stock for all items
        foreach (var item in order.Items)
        {
            var variant = await _context.ProductVariants.FindAsync(item.VariantId);
            if (variant != null) variant.Stock += item.Quantity;
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Cancelled,
            Notes = "تم الإلغاء من قبل العميل",
            ChangedByUserId = userId
        });

        await _context.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            userId, "❌ تم إلغاء طلبك",
            $"تم إلغاء طلبك #{orderId} بنجاح",
            NotificationType.OrderCancelled, $"/orders/{orderId}");

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        foreach (var admin in admins)
        {
            await _notificationService.CreateNotificationAsync(
                admin.Id,
                "❌ إلغاء طلب من العميل",
                $"قام العميل بإلغاء الطلب #{orderId} — تم إرجاع المخزون تلقائياً",
                NotificationType.OrderCancelled,
                $"/admin/adminorders/details/{orderId}");
        }
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
        => await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Variant)
            .Include(o => o.User)
            .Include(o => o.StatusHistory).ThenInclude(h => h.ChangedByUser)
            .Include(o => o.DeliveryAssignment).ThenInclude(a => a!.DeliveryUser)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<List<Order>> GetUserOrdersAsync(string userId)
        => await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IPagedList<Order>> GetAllOrdersAsync(
        OrderStatus? status, string? search, int page, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmed = search.Trim();
            if (int.TryParse(trimmed, out var orderId))
                query = query.Where(o => o.Id == orderId);
            else
                query = query.Where(o =>
                    o.User.FirstName.Contains(trimmed) ||
                    o.User.LastName.Contains(trimmed) ||
                    (o.CustomerPhone != null && o.CustomerPhone.Contains(trimmed)) ||
                    o.ShippingGovernorate.Contains(trimmed));
        }

        var totalCount = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new StaticPagedList<Order>(orders, page, pageSize, totalCount);
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = monthStart.AddSeconds(-1);

        return new DashboardViewModel
        {
            TodaySales = await _context.Orders
                .Where(o => o.CreatedAt.Date == today && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            MonthSales = await _context.Orders
                .Where(o => o.CreatedAt >= monthStart && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
            ShippedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipped),
            RecentOrders = await _context.Orders
                .Include(o => o.User).Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt).Take(10).ToListAsync(),
            TopProducts = await _context.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5).ToListAsync(),
            LowStockProducts = await _context.Products
                .Include(p => p.Variants)
                .Where(p => p.IsActive && p.Variants
                    .Any(v => !v.IsDeleted && v.IsActive && v.Stock <= 5))
                .Take(10).ToListAsync(),
            TotalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count,
            TotalActiveProducts = await _context.Products.CountAsync(p => p.IsActive && !p.IsDeleted),
            LastMonthSales = await _context.Orders
                .Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt <= lastMonthEnd
                            && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            TotalDeliveredOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Delivered),
            TotalCancelledOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Cancelled)
        };
    }
}
