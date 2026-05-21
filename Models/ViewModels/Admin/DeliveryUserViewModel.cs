namespace TellaStore.Models.ViewModels.Admin;

public class DeliveryUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalAssigned { get; set; }
    public int TotalDelivered { get; set; }
    public int TotalFailed { get; set; }
    public decimal TotalRevenue { get; set; }
}
