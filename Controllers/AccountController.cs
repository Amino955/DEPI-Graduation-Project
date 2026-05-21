using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Account;
using TellaStore.Settings;

namespace TellaStore.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IOptions<AppSettings> _settings;
    private readonly IEmailSender _emailSender;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context, IOptions<AppSettings> settings,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _settings = settings;
        _emailSender = emailSender;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

   [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
{
    if (!ModelState.IsValid) return View(model);

    // Check if user exists and is active BEFORE signing in
    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user != null && !user.IsActive)
    {
        ModelState.AddModelError(string.Empty, "هذا الحساب موقوف. تواصل مع الإدارة.");
        return View(model);
    }

    var result = await _signInManager.PasswordSignInAsync(
        model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

    if (result.Succeeded)
    {
        // If explicit returnUrl provided and local, go there
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        // Redirect based on role
        if (user != null)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "AdminDashboard", new { area = "Admin" });
            if (await _userManager.IsInRoleAsync(user, "Delivery"))
                return RedirectToAction("Index", "Delivery");
        }

        return RedirectToAction("Index", "Home");
    }

    if (result.IsLockedOut)
        ModelState.AddModelError(string.Empty, "الحساب محظور مؤقتاً بسبب محاولات متكررة. حاول مرة أخرى بعد 15 دقيقة");
    else
        ModelState.AddModelError(string.Empty, "بريد إلكتروني أو كلمة مرور غير صحيحة");

    return View(model);
}

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        
        var ordersCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);
        var wishlistCount = await _context.WishlistItems.CountAsync(w => w.UserId == user.Id);
        var addressesCount = await _context.Addresses.CountAsync(a => a.UserId == user.Id && !a.IsDeleted);

        return View(new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email!,
            CreatedAt = user.CreatedAt,
            OrdersCount = ordersCount,
            WishlistCount = wishlistCount,
            AddressesCount = addressesCount
        });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = "تم تحديث بياناتك بنجاح";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Addresses()
    {
        var userId = _userManager.GetUserId(User)!;
        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId && !a.IsDeleted).ToListAsync();
        return View(addresses);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(Address model)
    {
        var userId = _userManager.GetUserId(User)!;

        ModelState.Remove(nameof(model.UserId));
        ModelState.Remove(nameof(model.User));
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "البيانات المدخلة غير صالحة. يرجى تعبئة الحقول المطلوبة بشكل صحيح.";
            return RedirectToAction(nameof(Addresses));
        }

        var count = await _context.Addresses.CountAsync(a => a.UserId == userId && !a.IsDeleted);

        if (count >= _settings.Value.MaxAddressesPerUser)
        {
            TempData["Error"] = $"لا يمكن إضافة أكثر من {_settings.Value.MaxAddressesPerUser} عناوين";
            return RedirectToAction(nameof(Addresses));
        }

        if (model.IsDefault)
        {
            var existingAddresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            existingAddresses.ForEach(a => a.IsDefault = false);
        }

        model.UserId = userId;
        _context.Addresses.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إضافة العنوان بنجاح";
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address != null)
        {
            address.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> EditAddress(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId && !a.IsDeleted);
        if (address == null) return NotFound();
        return View(address);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(int id, Address model)
    {
        var userId = _userManager.GetUserId(User)!;
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId && !a.IsDeleted);
        if (address == null) return NotFound();

        ModelState.Remove(nameof(model.UserId));
        ModelState.Remove(nameof(model.User));
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.IsDefault)
        {
            var others = await _context.Addresses.Where(a => a.UserId == userId && a.Id != id).ToListAsync();
            others.ForEach(a => a.IsDefault = false);
        }

        address.Label = model.Label;
        address.Street = model.Street;
        address.City = model.City;
        address.Governorate = model.Governorate;
        address.IsDefault = model.IsDefault;
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تحديث العنوان بنجاح";
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(
            user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "تم تغيير كلمة المرور بنجاح";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "إعادة تعيين كلمة المرور",
                $"<p>لإعادة تعيين كلمة مرورك، <a href='{callbackUrl}'>اضغط هنا</a>.</p>");
        }

        TempData["Success"] = "إذا كان الحساب موجوداً، فقد تم إرسال رابط إعادة التعيين لبريدك الإلكتروني.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email)) return BadRequest();
        return View(new ResetPasswordViewModel { Token = token, Email = email });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            TempData["Success"] = "تم إعادة تعيين كلمة المرور بنجاح. يمكنك تسجيل الدخول الآن.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }


    [Route("access-denied")]
    public IActionResult AccessDenied() => View();
}
