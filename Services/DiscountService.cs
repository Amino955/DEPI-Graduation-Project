using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Services.Interfaces;

namespace TellaStore.Services;

public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _context;

    public DiscountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetDiscountedPriceAsync(int productId, decimal originalPrice)
    {
        var discount = await GetActiveDiscountForProductAsync(productId);
        if (discount == null) return originalPrice;

        var discountedPrice = discount.Type == DiscountType.Percentage
            ? Math.Round(originalPrice * (1 - discount.Value / 100), 2)
            : originalPrice - discount.Value;

        return Math.Max(0, discountedPrice);
    }

    public async Task<Discount?> GetActiveDiscountForProductAsync(int productId)
    {
        var categoryId = await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => p.CategoryId)
            .FirstOrDefaultAsync();

        return await _context.Discounts
            .Where(d => d.IsActive
                && !d.IsDeleted
                && DateTime.UtcNow >= d.StartDate
                && DateTime.UtcNow <= d.EndDate
                && (d.ProductId == productId
                    || d.CategoryId == categoryId
                    || d.Target == DiscountTarget.AllStore))
            .OrderBy(d => d.Target)     // Product(0) > Category(1) > AllStore(2)
            .ThenByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Discount>> GetAllDiscountsAsync()
        => await _context.Discounts
            .Where(d => !d.IsDeleted)
            .Include(d => d.Product)
            .Include(d => d.Category)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

    public async Task<Discount?> GetDiscountByIdAsync(int id)
        => await _context.Discounts
            .Include(d => d.Product)
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

    public async Task<Discount> CreateDiscountAsync(DiscountFormViewModel model)
    {
        if (!model.StartDate.HasValue || !model.EndDate.HasValue)
            throw new ArgumentException("تاريخ البدء وتاريخ الانتهاء مطلوبان");

        if (model.EndDate <= model.StartDate)
            throw new ArgumentException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية");

        if (model.Target == DiscountTarget.Product && !model.ProductId.HasValue)
            throw new ArgumentException("يجب اختيار منتج للخصم");

        if (model.Target == DiscountTarget.Category && !model.CategoryId.HasValue)
            throw new ArgumentException("يجب اختيار قسم للخصم");

        if (model.Type == DiscountType.Percentage && (model.Value < 0 || model.Value > 100))
            throw new ArgumentException("نسبة الخصم يجب أن تكون بين 0 و 100");

        var discount = new Discount
        {
            Name = model.Name,
            Target = model.Target,
            ProductId = model.ProductId,
            CategoryId = model.CategoryId,
            Type = model.Type,
            Value = model.Value,
            StartDate = model.StartDate.Value,
            EndDate = model.EndDate.Value,
            IsActive = model.IsActive
        };
        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();
        return discount;
    }

    public async Task UpdateDiscountAsync(int id, DiscountFormViewModel model)
    {
        if (!model.StartDate.HasValue || !model.EndDate.HasValue)
            throw new ArgumentException("تاريخ البدء وتاريخ الانتهاء مطلوبان");

        if (model.EndDate <= model.StartDate)
            throw new ArgumentException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية");

        if (model.Target == DiscountTarget.Product && !model.ProductId.HasValue)
            throw new ArgumentException("يجب اختيار منتج للخصم");

        if (model.Target == DiscountTarget.Category && !model.CategoryId.HasValue)
            throw new ArgumentException("يجب اختيار قسم للخصم");

        if (model.Type == DiscountType.Percentage && (model.Value < 0 || model.Value > 100))
            throw new ArgumentException("نسبة الخصم يجب أن تكون بين 0 و 100");

        var discount = await _context.Discounts.FindAsync(id)
            ?? throw new Exception("الخصم غير موجود");

        discount.Name = model.Name;
        discount.Target = model.Target;
        discount.ProductId = model.ProductId;
        discount.CategoryId = model.CategoryId;
        discount.Type = model.Type;
        discount.Value = model.Value;
        discount.StartDate = model.StartDate.Value;
        discount.EndDate = model.EndDate.Value;
        discount.IsActive = model.IsActive;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDiscountAsync(int id)
    {
        var discount = await _context.Discounts.FindAsync(id)
            ?? throw new Exception("الخصم غير موجود");
        discount.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var discount = await _context.Discounts.FindAsync(id)
            ?? throw new Exception("الخصم غير موجود");
        discount.IsActive = !discount.IsActive;
        await _context.SaveChangesAsync();
    }
}
