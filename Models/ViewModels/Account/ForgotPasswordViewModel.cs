using System.ComponentModel.DataAnnotations;

namespace TellaStore.Models.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
    public string Email { get; set; } = string.Empty;
}
