using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class Category : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    // Self-referencing for subcategories (رجالي → تيشيرتات)
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
