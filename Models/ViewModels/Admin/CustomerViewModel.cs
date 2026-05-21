namespace TellaStore.Models.ViewModels.Admin;

public class CustomerViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
}
