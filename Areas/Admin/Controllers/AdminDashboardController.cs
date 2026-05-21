using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TellaStore.Data;
using TellaStore.Models.ViewModels.Admin;
using TellaStore.Services.Interfaces;

namespace TellaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminDashboardController : AdminBaseController
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;

    public AdminDashboardController(IOrderService orderService, IProductService productService, ApplicationDbContext context) : base(context)
    {
        _orderService = orderService;
        _productService = productService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await _orderService.GetDashboardDataAsync();
        ViewBag.LowStockCount = model.LowStockProducts.Count;
        return View(model);
    }
}
