using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class Review : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsApproved { get; set; } = true;
}
