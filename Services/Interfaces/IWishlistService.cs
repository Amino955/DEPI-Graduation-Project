using TellaStore.Models.Entities;

namespace TellaStore.Services.Interfaces;

public interface IWishlistService
{
    Task<bool> ToggleAsync(string userId, int productId);
    Task<List<Product>> GetWishlistAsync(string userId);
    Task<bool> IsInWishlistAsync(string userId, int productId);
Task<int> GetCountAsync(string userId);
Task<HashSet<int>> GetWishlistProductIdsAsync(string userId);
Task ClearWishlistAsync(string userId);
}
