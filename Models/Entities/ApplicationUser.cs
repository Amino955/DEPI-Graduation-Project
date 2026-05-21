using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TellaStore.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
