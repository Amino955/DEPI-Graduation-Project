using TellaStore.Models.Entities;

namespace TellaStore.Models.ViewModels.Products;

public class ProductFilterViewModel
{
    public int? CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public string? SearchTerm { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public Season? Season { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFeatured { get; set; }
    public string SortBy { get; set; } = "newest";
    public int Page { get; set; } = 1;
}
