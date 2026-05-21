namespace TellaStore.Models.ViewModels.Cart;

public class CartItemViewModel
{
    public int VariantId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public string Size { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => FinalPrice * Quantity;
    public string ImageUrl { get; set; } = string.Empty;
    public int AvailableStock { get; set; }
}
