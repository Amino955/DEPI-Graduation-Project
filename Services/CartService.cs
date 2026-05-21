using TellaStore.Models.ViewModels.Cart;
using TellaStore.Services.Interfaces;

namespace TellaStore.Services;

public class CartService : ICartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CartKey = "tella_cart";

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    private List<CartItemViewModel> LoadCart()
    {
        var json = Session.GetString(CartKey);
        return string.IsNullOrEmpty(json)
            ? new List<CartItemViewModel>()
            : System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(json)!;
    }

    private void SaveCart(List<CartItemViewModel> cart)
        => Session.SetString(CartKey, System.Text.Json.JsonSerializer.Serialize(cart));

    public void AddItem(int productId, int variantId, string productName,
        string color, string colorHex, string size, decimal originalPrice,
        decimal finalPrice, int quantity, string imageUrl)
    {
        var cart = LoadCart();
        var existing = cart.FirstOrDefault(i => i.VariantId == variantId);

        if (existing != null)
            existing.Quantity += quantity;
        else
            cart.Add(new CartItemViewModel
            {
                ProductId = productId,
                VariantId = variantId,
                ProductName = productName,
                Color = color,
                ColorHex = colorHex,
                Size = size,
                UnitPrice = originalPrice,
                FinalPrice = finalPrice,
                Quantity = quantity,
                ImageUrl = imageUrl
            });

        SaveCart(cart);
    }

    public void RemoveItem(int variantId)
    {
        var cart = LoadCart();
        cart.RemoveAll(i => i.VariantId == variantId);
        SaveCart(cart);
    }

    public void UpdateQuantity(int variantId, int quantity)
    {
        if (quantity <= 0) { RemoveItem(variantId); return; }
        var cart = LoadCart();
        var item = cart.FirstOrDefault(i => i.VariantId == variantId);
        if (item != null) { item.Quantity = quantity; SaveCart(cart); }
    }

    public List<CartItemViewModel> GetCart() => LoadCart();
    public void ClearCart() => Session.Remove(CartKey);
    public decimal GetTotal() => LoadCart().Sum(i => i.FinalPrice * i.Quantity);
    public int GetItemCount() => LoadCart().Sum(i => i.Quantity);
    public bool IsEmpty() => !LoadCart().Any();
}
