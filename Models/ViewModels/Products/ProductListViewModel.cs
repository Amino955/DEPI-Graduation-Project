using TellaStore.Models.Entities;
using X.PagedList;

namespace TellaStore.Models.ViewModels.Products;

public class ProductListViewModel
{
    public IPagedList<ProductCardViewModel> Products { get; set; } = null!;
    public ProductFilterViewModel Filter { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<string> AvailableColors { get; set; } = new();
    public List<string> AvailableSizes { get; set; } = new();
}
