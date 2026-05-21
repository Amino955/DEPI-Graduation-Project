using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.ViewModels.Account;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [MinLength(6, ErrorMessage = "يجب أن تكون كلمة المرور 6 أحرف على الأقل")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "كلمة المرور غير متطابقة")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
