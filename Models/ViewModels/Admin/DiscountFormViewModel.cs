using System.ComponentModel.DataAnnotations;
using TellaStore.Models.Entities;

namespace TellaStore.Models.ViewModels.Admin;

public class DiscountFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "اسم الخصم مطلوب")]
    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public DiscountTarget Target { get; set; }
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }
    public DiscountType Type { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "القيمة يجب أن تكون أكبر من صفر")]
    public decimal Value { get; set; }

    public decimal? MinimumOrderAmount { get; set; }

    public DateTime? StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; } = DateTime.Today.AddDays(7);
    public bool IsActive { get; set; } = true;

    public int? UsageCount { get; set; }

    // Lists for the form dropdowns / checklists
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    // Selected IDs from the checkboxes
    public List<int> SelectedProductIds { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
}
