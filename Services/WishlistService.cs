using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Services.Interfaces;

namespace TellaStore.Services;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;

    public WishlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ToggleAsync(string userId, int productId)
    {
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing != null)
        {
            _context.WishlistItems.Remove(existing);
            await _context.SaveChangesAsync();
            return false; // Removed
        }

        _context.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await _context.SaveChangesAsync();
        return true; // Added
    }

    public async Task<List<Product>> GetWishlistAsync(string userId)
        => await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .Include(w => w.Product).ThenInclude(p => p.Variants)
            .Select(w => w.Product)
            .Where(p => !p.IsDeleted && p.IsActive)
            .ToListAsync();

    public async Task<bool> IsInWishlistAsync(string userId, int productId)
        => await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);






public async Task<HashSet<int>> GetWishlistProductIdsAsync(string userId)
{
    var ids = await _context.WishlistItems
        .Where(w => w.UserId == userId)
        .Select(w => w.ProductId)
        .ToListAsync();
    return ids.ToHashSet();
}


    public async Task<int> GetCountAsync(string userId)
        => await _context.WishlistItems.CountAsync(w => w.UserId == userId);



        public async Task ClearWishlistAsync(string userId)
{
    var items = await _context.WishlistItems
        .Where(w => w.UserId == userId)
        .ToListAsync();
    _context.WishlistItems.RemoveRange(items);
    await _context.SaveChangesAsync();
}

}
