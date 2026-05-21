using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Products;


namespace TellaStore.Models.ViewModels.Home;

public class HomeViewModel
{
    public List<ProductCardViewModel> FeaturedProducts { get; set; } = new();
    public List<ProductCardViewModel> LatestProducts { get; set; } = new();
    public List<Category> ParentCategories { get; set; } = new();
    public List<Discount> ActiveDiscounts { get; set; } = new();
}
