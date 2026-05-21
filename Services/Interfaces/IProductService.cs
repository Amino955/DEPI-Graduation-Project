using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Models.ViewModels.Products;
using X.PagedList;

namespace TellaStore.Services.Interfaces;

public interface IProductService
{
    Task<IPagedList<Product>> GetProductsAsync(
        int? categoryId, string? searchTerm, string? color, string? size,
        Season? season, decimal? minPrice, decimal? maxPrice,
        bool? isFeatured, bool includeInactive, string sortBy, int pageNumber, int pageSize);

    Task<Product?> GetProductByIdAsync(int id);
    Task<Product?> GetProductBySlugAsync(string slug);
    Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
    Task<List<Product>> GetLatestProductsAsync(int count = 8);
    Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4);
    Task<Product> CreateProductAsync(ProductFormViewModel model);
    Task<Product> UpdateProductAsync(int id, ProductFormViewModel model);
    Task DeleteProductAsync(int id);
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
    Task<List<Product>> GetLowStockProductsAsync(int threshold = 5);
    Task<List<string>> GetAvailableColorsAsync(int? categoryId = null);
    Task<List<string>> GetAvailableSizesAsync(int? categoryId = null);
    Task<ProductCardViewModel> ToCardViewModelAsync(Product product);
}
