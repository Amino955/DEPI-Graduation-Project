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
public class AdminCustomersController : AdminBaseController
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminCustomersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : base(context)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? filter)
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        if (!string.IsNullOrWhiteSpace(search))
            customers = customers.Where(u =>
                u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (u.Email != null && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
            ).ToList();

        // فلترة حسب الحالة
        if (filter == "active")
            customers = customers.Where(u => u.IsActive).ToList();
        else if (filter == "inactive")
            customers = customers.Where(u => !u.IsActive).ToList();
        // "all" أو null = كل العملاء بدون فلترة

        // Fetch stats in a single batch query to eliminate N+1
        var customerIds = customers.Select(c => c.Id).ToList();
        var orderStats = await _context.Orders
            .Where(o => customerIds.Contains(o.UserId))
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                OrdersCount = g.Count(),
                TotalSpent = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total)
            })
            .ToDictionaryAsync(x => x.UserId, x => x);

        // أضف إحصائيات لكل عميل
        var result = new List<CustomerViewModel>();
        foreach (var user in customers.OrderByDescending(u => u.CreatedAt))
        {
            int ordersCount = 0;
            decimal totalSpent = 0;

            if (orderStats.TryGetValue(user.Id, out var stats))
            {
                ordersCount = stats.OrdersCount;
                totalSpent = stats.TotalSpent;
            }

            result.Add(new CustomerViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                OrdersCount = ordersCount,
                TotalSpent = totalSpent
            });
        }

        ViewBag.Search = search;
        ViewBag.ShowInactive = filter;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var orders = await _context.Orders
            .Where(o => o.UserId == id)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var addresses = await _context.Addresses
            .Where(a => a.UserId == id && !a.IsDeleted)
            .ToListAsync();

        ViewBag.User = user;
        ViewBag.Orders = orders;
        ViewBag.Addresses = addresses;
        ViewBag.TotalSpent = orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Sum(o => o.Total);
        ViewBag.OrdersCount = orders.Count;
        return View();
    }

    // تعطيل / تفعيل حساب عميل
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "لا يمكن تعطيل حساب الأدمن";
            return RedirectToAction(nameof(Index));
        }
        
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        
        // لو تم تعطيله، أنهي جلساته
        if (!user.IsActive)
            await _userManager.UpdateSecurityStampAsync(user);
        
        TempData["Success"] = user.IsActive ? "تم تفعيل الحساب" : "تم تعطيل الحساب";
        return RedirectToAction(nameof(Index));
    }

    // حذف نهائي للحساب (مع حماية)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "لا يمكن حذف حساب الأدمن";
            return RedirectToAction(nameof(Index));
        }
        
        var hasActiveOrders = await _context.Orders
            .AnyAsync(o => o.UserId == id && 
                o.Status != OrderStatus.Delivered && 
                o.Status != OrderStatus.Cancelled);
        
        if (hasActiveOrders)
        {
            TempData["Error"] = "لا يمكن حذف العميل لديه طلبات جارية";
            return RedirectToAction(nameof(Index));
        }
        
        await _userManager.DeleteAsync(user);
        TempData["Success"] = "تم حذف الحساب بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
