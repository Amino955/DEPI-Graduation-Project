using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "الاسم الأول مطلوب")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الأخير مطلوب")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(8, ErrorMessage = "كلمة المرور 8 أحرف على الأقل")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
