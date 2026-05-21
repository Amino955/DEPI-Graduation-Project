using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TellaStore.Models.Entities;

namespace TellaStore.Models.ViewModels.Admin;

public class ProductFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [MaxLength(200, ErrorMessage = "الاسم لا يتجاوز 200 حرف")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "السعر مطلوب")]
    [Range(0.01, 99999.99, ErrorMessage = "السعر بين 0.01 و 99999.99")]
    public decimal BasePrice { get; set; }

    [Required(ErrorMessage = "القسم مطلوب")]
    public int CategoryId { get; set; }

    public Season Season { get; set; } = Season.AllSeason;
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    public List<VariantFormViewModel> Variants { get; set; } = new();
    public List<IFormFile> NewImages { get; set; } = new();
    public List<int> DeleteImageIds { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}

public class VariantFormViewModel
{
    public int? Id { get; set; }
    [Required] public string Color { get; set; } = string.Empty;
    [Required] public string ColorHex { get; set; } = "#000000";
    [Required] public string Size { get; set; } = string.Empty;
    public SizeType SizeType { get; set; } = SizeType.Letters;
    [Range(0, 99999)] public int Stock { get; set; }
    [Range(0, 99999)] public decimal PriceModifier { get; set; }
}
