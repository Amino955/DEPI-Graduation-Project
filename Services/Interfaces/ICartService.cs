using TellaStore.Models.ViewModels.Cart;

namespace TellaStore.Services.Interfaces;

public interface ICartService
{
    void AddItem(int productId, int variantId, string productName,
                 string color, string colorHex, string size,
                 decimal originalPrice, decimal finalPrice, int quantity, string imageUrl);
    void RemoveItem(int variantId);
    void UpdateQuantity(int variantId, int quantity);
    List<CartItemViewModel> GetCart();
    void ClearCart();
    decimal GetTotal();
    int GetItemCount();
    bool IsEmpty();
}
