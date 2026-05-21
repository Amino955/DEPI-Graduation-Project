using System.ComponentModel.DataAnnotations;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Cart;

namespace TellaStore.Models.ViewModels.Orders;

public class CheckoutViewModel
{
    public int? SelectedAddressId { get; set; }
    public List<Address> SavedAddresses { get; set; } = new();

    [MaxLength(200)]
    public string? NewStreet { get; set; }
    [MaxLength(100)]
    public string? NewCity { get; set; }
    [MaxLength(100)]
    public string? NewGovernorate { get; set; }
    public bool SaveNewAddress { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب للتواصل معك عند التوصيل")]
    [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "يرجى إدخال رقم موبايل مصري صحيح (مثال: 01012345678)")]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public CartViewModel Cart { get; set; } = new();
}
