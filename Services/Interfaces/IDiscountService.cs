using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;

namespace TellaStore.Services.Interfaces;

public interface IDiscountService
{
    Task<decimal> GetDiscountedPriceAsync(int productId, decimal originalPrice);
    Task<Discount?> GetActiveDiscountForProductAsync(int productId);
    Task<List<Discount>> GetAllDiscountsAsync();
    Task<Discount?> GetDiscountByIdAsync(int id);
    Task<Discount> CreateDiscountAsync(DiscountFormViewModel model);
    Task UpdateDiscountAsync(int id, DiscountFormViewModel model);
    Task DeleteDiscountAsync(int id);
    Task ToggleActiveAsync(int id);
}
