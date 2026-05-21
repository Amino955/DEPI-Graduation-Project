using Microsoft.EntityFrameworkCore;
using X.PagedList;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Services.Interfaces;
using TellaStore.Helpers;
using TellaStore.Models.ViewModels.Products; 
namespace TellaStore.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IDiscountService _discountService;

    public ProductService(ApplicationDbContext context,
        IFileService fileService, IDiscountService discountService)
    {
        _context = context;
        _fileService = fileService;
        _discountService = discountService;
    }

    public async Task<IPagedList<Product>> GetProductsAsync(
        int? categoryId, string? searchTerm, string? color, string? size,
        Season? season, decimal? minPrice, decimal? maxPrice,
        bool? isFeatured, bool includeInactive, string sortBy, int pageNumber, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // when includeInactive is false, only show active products
        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (isFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == isFeatured.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId
                || p.Category.ParentCategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm)
                || (p.Description != null && p.Description.Contains(searchTerm)));

        if (!string.IsNullOrWhiteSpace(color))
            query = query.Where(p => p.Variants.Any(v => v.Color == color && !v.IsDeleted && v.IsActive));

        if (!string.IsNullOrWhiteSpace(size))
            query = query.Where(p => p.Variants.Any(v => v.Size == size && !v.IsDeleted && v.IsActive));

        if (season.HasValue)
            query = query.Where(p => p.Season == season || p.Season == Season.AllSeason);

        if (minPrice.HasValue)
            query = query.Where(p => p.BasePrice >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= maxPrice);

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.BasePrice),
            "price_desc" => query.OrderByDescending(p => p.BasePrice),
            "name" => query.OrderBy(p => p.Name),
            "top_rated" => query.OrderByDescending(p =>
                                p.Reviews.Where(r => r.IsApproved).Average(r => (double?)r.Rating) ?? 0),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new StaticPagedList<Product>(products, pageNumber, pageSize, totalCount);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
        => await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetProductBySlugAsync(string slug)
        => await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .Include(p => p.Reviews.Where(r => r.IsApproved))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
        => await _context.Products
            .Where(p => !p.IsDeleted && p.IsFeatured && p.IsActive)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count).ToListAsync();

    public async Task<List<Product>> GetLatestProductsAsync(int count = 8)
        => await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count).ToListAsync();

    public async Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return new List<Product>();
        return await _context.Products
            .Where(p => !p.IsDeleted && p.CategoryId == product.CategoryId && p.Id != productId && p.IsActive)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count).ToListAsync();
    }

    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 5)
        => await _context.Products
            .Include(p => p.Variants)
            .Where(p => p.IsActive && p.Variants
                .Any(v => !v.IsDeleted && v.IsActive && v.Stock <= threshold))
            .ToListAsync();

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        var query = _context.Products.Where(p => p.Slug == slug);
        if (excludeId.HasValue) query = query.Where(p => p.Id != excludeId);
        return !await query.AnyAsync();
    }

    public async Task<List<string>> GetAvailableColorsAsync(int? categoryId = null)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => !v.IsDeleted && v.IsActive && !v.Product.IsDeleted && v.Product.IsActive);
        if (categoryId.HasValue)
            query = query.Where(v => v.Product.CategoryId == categoryId);
        return await query.Select(v => v.Color).Distinct().OrderBy(c => c).ToListAsync();
    }

    public async Task<List<string>> GetAvailableSizesAsync(int? categoryId = null)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => !v.IsDeleted && v.IsActive && !v.Product.IsDeleted && v.Product.IsActive);
        if (categoryId.HasValue)
            query = query.Where(v => v.Product.CategoryId == categoryId);
        return await query.Select(v => v.Size).Distinct().OrderBy(s => s).ToListAsync();
    }

    public async Task<Product> CreateProductAsync(ProductFormViewModel model)
    {
        // generate slug and ensure uniqueness (with retries to avoid DB unique index conflicts)
        var baseSlug = SlugHelper.GenerateSlug(model.Name);
        var slug = baseSlug;
        int suffix = 0;
        // quick check to avoid obvious duplicates
        while (!await IsSlugUniqueAsync(slug) && suffix < 10)
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        var product = new Product
        {
            Name = model.Name,
            Slug = slug,
            Description = model.Description,
            BasePrice = model.BasePrice,
            CategoryId = model.CategoryId,
            Season = model.Season,
            IsFeatured = model.IsFeatured,
            IsActive = model.IsActive
        };

        foreach (var v in model.Variants)
            product.Variants.Add(new ProductVariant
            {
                Color = v.Color,
                ColorHex = v.ColorHex,
                Size = v.Size,
                SizeType = v.SizeType,
                Stock = v.Stock,
                PriceModifier = v.PriceModifier,
                SKU = $"TELLA-{DateTime.UtcNow.Ticks}-{Random.Shared.Next(1000, 10000)}-{v.Color}-{v.Size}".ToUpper().Replace(" ", "-")
            });

        _context.Products.Add(product);
        // attempt save with retry in case of race condition causing duplicate slug
        int attempts = 0;
        const int maxAttempts = 5;
        while (true)
        {
            try
            {
                await _context.SaveChangesAsync();
                break;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                attempts++;
                if (attempts >= maxAttempts)
                    throw;
                // likely duplicate slug index conflict, regenerate slug and retry
                slug = $"{baseSlug}-{DateTime.UtcNow.Ticks}";
                product.Slug = slug;
            }
        }

        // Upload images to Cloudinary
        if (model.NewImages?.Any() == true)
        {
            var uploadResults = await _fileService.UploadMultipleAsync(model.NewImages, "tella/products");
            bool isFirst = true;
            foreach (var (url, publicId) in uploadResults)
            {
                product.Images.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    PublicId = publicId,
                    IsMain = isFirst,
                    SortOrder = product.Images.Count
                });
                isFirst = false;
            }
            await _context.SaveChangesAsync();
        }

        return product;
    }

    public async Task<Product> UpdateProductAsync(int id, ProductFormViewModel model)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new Exception("المنتج غير موجود");

        product.Name = model.Name;
        product.Description = model.Description;
        product.BasePrice = model.BasePrice;
        product.CategoryId = model.CategoryId;
        product.Season = model.Season;
        product.IsFeatured = model.IsFeatured;
        product.IsActive = model.IsActive;

        // Delete images that were marked for deletion
        if (model.DeleteImageIds?.Any() == true)
        {
            var toDelete = product.Images.Where(i => model.DeleteImageIds.Contains(i.Id)).ToList();
            foreach (var img in toDelete)
            {
                if (!string.IsNullOrEmpty(img.PublicId))
                    await _fileService.DeleteImageAsync(img.PublicId);
                _context.ProductImages.Remove(img);
            }
        }

        // Upload new images
        if (model.NewImages?.Any() == true)
        {
            var uploadResults = await _fileService.UploadMultipleAsync(model.NewImages, "tella/products");
            foreach (var (url, publicId) in uploadResults)
                product.Images.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    PublicId = publicId,
                    IsMain = !product.Images.Any(i => i.IsMain),
                    SortOrder = product.Images.Count
                });
        }

        // ── Variant Sync ────────────────────────────────────────────
        // Collect IDs of variants submitted in the form
        var submittedIds = model.Variants
            .Where(v => v.Id.HasValue)
            .Select(v => v.Id!.Value)
            .ToHashSet();

        // Soft-delete variants not present in the submitted form
        foreach (var existing in product.Variants.Where(v => !v.IsDeleted))
        {
            if (!submittedIds.Contains(existing.Id))
                existing.IsDeleted = true;
        }

        // Update existing variants and add new ones
        foreach (var vModel in model.Variants)
        {
            if (vModel.Id.HasValue)
            {
                // Update existing
                var existing = product.Variants.FirstOrDefault(v => v.Id == vModel.Id.Value);
                if (existing != null)
                {
                    existing.Color = vModel.Color;
                    existing.ColorHex = vModel.ColorHex;
                    existing.Size = vModel.Size;
                    existing.SizeType = vModel.SizeType;
                    existing.Stock = vModel.Stock;
                    existing.PriceModifier = vModel.PriceModifier;
                    existing.IsDeleted = false;
                    existing.IsActive = true;
                }
            }
            else
            {
                // Add new variant
                product.Variants.Add(new ProductVariant
                {
                    Color = vModel.Color,
                    ColorHex = vModel.ColorHex,
                    Size = vModel.Size,
                    SizeType = vModel.SizeType,
                    Stock = vModel.Stock,
                    PriceModifier = vModel.PriceModifier,
                    IsActive = true,
                    SKU = $"TELLA-{DateTime.UtcNow.Ticks}-{Random.Shared.Next(1000, 10000)}-{vModel.Color}-{vModel.Size}"
                            .ToUpper().Replace(" ", "-")
                });
            }
        }
        // ── End Variant Sync ─────────────────────────────────────────

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new Exception("المنتج غير موجود");
        product.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<ProductCardViewModel> ToCardViewModelAsync(Product product)
    {
        var finalPrice = await _discountService.GetDiscountedPriceAsync(product.Id, product.BasePrice);
        return new ProductCardViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            BasePrice = product.BasePrice,
            FinalPrice = finalPrice,
            MainImageUrl = product.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl 
                         ?? product.Images?.FirstOrDefault()?.ImageUrl,
            IsInStock = product.Variants.Any(v => v.Stock > 0 && !v.IsDeleted && v.IsActive),
            IsFeatured = product.IsFeatured,
            AverageRating = product.Reviews?.Any(r => r.IsApproved) == true
                ? product.Reviews.Where(r => r.IsApproved).Average(r => r.Rating)
                : 0,
            ReviewCount = product.Reviews?.Count(r => r.IsApproved) ?? 0
        };
    }
}
