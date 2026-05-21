using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class WishlistItem : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
