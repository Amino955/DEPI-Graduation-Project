using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;
using TellaStore.Models.ViewModels.Admin;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminDeliveryController : AdminBaseController
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDeliveryController(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : base(context)
    {
        _userManager = userManager;
    }

    // قائمة المندوبين مع إحصائياتهم
    public async Task<IActionResult> Index()
    {
        var deliveryUsers = await _userManager.GetUsersInRoleAsync("Delivery");
        var deliveryUserIds = deliveryUsers.Select(u => u.Id).ToList();

        // Batch query all assignments for the delivery users to eliminate N+1
        var allAssignments = await _context.DeliveryAssignments
            .Where(a => deliveryUserIds.Contains(a.DeliveryUserId))
            .Include(a => a.Order)
            .ToListAsync();

        var assignmentsMap = allAssignments
            .GroupBy(a => a.DeliveryUserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<DeliveryUserViewModel>();

        foreach (var user in deliveryUsers)
        {
            var assignments = assignmentsMap.TryGetValue(user.Id, out var list)
                ? list
                : new List<DeliveryAssignment>();

            result.Add(new DeliveryUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                TotalAssigned = assignments.Count,
                TotalDelivered = assignments.Count(a => a.Status == DeliveryAssignmentStatus.Delivered),
                TotalFailed = assignments.Count(a => a.Status == DeliveryAssignmentStatus.Failed),
                TotalRevenue = assignments
                    .Where(a => a.Status == DeliveryAssignmentStatus.Delivered)
                    .Sum(a => a.Order.Total)
            });
        }
        return View(result);
    }

    // تفاصيل مندوب
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !await _userManager.IsInRoleAsync(user, "Delivery"))
            return NotFound();

        var assignments = await _context.DeliveryAssignments
            .Where(a => a.DeliveryUserId == id)
            .Include(a => a.Order).ThenInclude(o => o.User)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        ViewBag.User = user;
        return View(assignments);
    }

    // تفعيل / تعطيل مندوب
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = user.IsActive ? "تم تفعيل المندوب" : "تم تعطيل المندوب";
        return RedirectToAction(nameof(Index));
    }

    // إضافة مندوب جديد (تعيين دور Delivery لعميل موجود)
    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            TempData["Error"] = "المستخدم غير موجود";
            return RedirectToAction(nameof(Create));
        }
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "لا يمكن تعيين الأدمن كمندوب";
            return RedirectToAction(nameof(Create));
        }
        if (!await _userManager.IsInRoleAsync(user, "Delivery"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Customer");
            await _userManager.AddToRoleAsync(user, "Delivery");
        }
        TempData["Success"] = $"تم تعيين {user.FullName} كمندوب توصيل";
        return RedirectToAction(nameof(Index));
    }

    // إزالة دور المندوب (إرجاعه عميل)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDeliveryRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        await _userManager.RemoveFromRoleAsync(user, "Delivery");
        await _userManager.AddToRoleAsync(user, "Customer");
        TempData["Success"] = "تم إزالة دور التوصيل من المندوب";
        return RedirectToAction(nameof(Index));
    }
}
