using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.Entities;

public class Address : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Label { get; set; } = string.Empty;       // e.g. البيت، الشغل

    [Required, MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Governorate { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsDefault { get; set; } = false;
}
