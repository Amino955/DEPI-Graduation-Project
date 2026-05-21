namespace TellaStore.Models.ViewModels.Cart;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total => SubTotal - DiscountAmount;
    public int ItemCount => Items.Count;
}
