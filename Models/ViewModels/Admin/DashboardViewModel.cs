using TellaStore.Models.Entities;

namespace TellaStore.Models.ViewModels.Admin;

public class DashboardViewModel
{
    public decimal TodaySales { get; set; }
    public decimal MonthSales { get; set; }
    public int PendingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<TopProductViewModel> TopProducts { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();
    
    public int TotalCustomers { get; set; }
    public int TotalActiveProducts { get; set; }
    public decimal LastMonthSales { get; set; }
    public int TotalDeliveredOrders { get; set; }
    public int TotalCancelledOrders { get; set; }
}

public class TopProductViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
}
