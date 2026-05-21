using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.ViewModels.Account;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [MinLength(8, ErrorMessage = "كلمة المرور لا تقل عن 8 أحرف")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
