using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TellaStore.Data;
using TellaStore.Models.Entities;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class AdminBaseController : Controller
{
    protected readonly ApplicationDbContext _context;

    protected AdminBaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewBag.PendingOrdersCount = await _context.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending);
        await next();
    }
}
