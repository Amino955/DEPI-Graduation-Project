using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.ViewModels.Account;

public class ProfileViewModel
{
    [Required(ErrorMessage = "الاسم الأول مطلوب")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الأخير مطلوب")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public int OrdersCount { get; set; }
    public int WishlistCount { get; set; }
    public int AddressesCount { get; set; }
}
