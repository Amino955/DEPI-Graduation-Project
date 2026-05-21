using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TellaStore.Models.Entities;

namespace TellaStore.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // --- Create Roles ---
        string[] roles = { "Admin", "Delivery", "Customer" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // --- Create Default Admin User ---
        if (await userManager.FindByEmailAsync("admin@tella.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@tella.com",
                Email = "admin@tella.com",
                FirstName = "مدير",
                LastName = "المتجر",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // --- Create Delivery User ---
        if (await userManager.FindByEmailAsync("delivery@tella.com") == null)
        {
            var delivery = new ApplicationUser
            {
                UserName = "delivery@tella.com",
                Email = "delivery@tella.com",
                FirstName = "موصل",
                LastName = "الطلبات",
                EmailConfirmed = true,
                IsActive = true
            };
            var res2 = await userManager.CreateAsync(delivery, "Delivery@12345");
            if (res2.Succeeded)
                await userManager.AddToRoleAsync(delivery, "Delivery");
        }

        // --- Create Customer User ---
        if (await userManager.FindByEmailAsync("customer@tella.com") == null)
        {
            var customer = new ApplicationUser
            {
                UserName = "customer@tella.com",
                Email = "customer@tella.com",
                FirstName = "عميل",
                LastName = "تجريبي",
                EmailConfirmed = true,
                IsActive = true
            };
            var res3 = await userManager.CreateAsync(customer, "Customer@12345");
            if (res3.Succeeded)
                await userManager.AddToRoleAsync(customer, "Customer");
        }

        // --- Create Categories (only if none exist) ---
        if (!context.Categories.Any())
        {
            var parents = new List<Category>
            {
                new() { Name = "رجالي", Slug = "men", SortOrder = 1 },
                new() { Name = "حريمي", Slug = "women", SortOrder = 2 },
                new() { Name = "أطفال", Slug = "kids", SortOrder = 3 },
            };
            context.Categories.AddRange(parents);
            await context.SaveChangesAsync();

            var men = context.Categories.First(c => c.Slug == "men");
            context.Categories.AddRange(new List<Category>
            {
                new() { Name = "تيشيرتات", Slug = "men-tshirts", ParentCategoryId = men.Id, SortOrder = 1 },
                new() { Name = "قمصان", Slug = "men-shirts", ParentCategoryId = men.Id, SortOrder = 2 },
                new() { Name = "بناطيل جينز", Slug = "men-jeans", ParentCategoryId = men.Id, SortOrder = 3 },
                new() { Name = "جاكيتات", Slug = "men-jackets", ParentCategoryId = men.Id, SortOrder = 4 },
                new() { Name = "ملابس رياضية", Slug = "men-sports", ParentCategoryId = men.Id, SortOrder = 5 },
            });

            var women = context.Categories.First(c => c.Slug == "women");
            context.Categories.AddRange(new List<Category>
            {
                new() { Name = "فساتين", Slug = "women-dresses", ParentCategoryId = women.Id, SortOrder = 1 },
                new() { Name = "بلوزات", Slug = "women-tops", ParentCategoryId = women.Id, SortOrder = 2 },
                new() { Name = "بناطيل", Slug = "women-pants", ParentCategoryId = women.Id, SortOrder = 3 },
                new() { Name = "تنانير", Slug = "women-skirts", ParentCategoryId = women.Id, SortOrder = 4 },
            });

            var kids = context.Categories.First(c => c.Slug == "kids");
            context.Categories.AddRange(new List<Category>
            {
                new() { Name = "رضع (0-2 سنة)", Slug = "kids-baby", ParentCategoryId = kids.Id, SortOrder = 1 },
                new() { Name = "أطفال (2-8 سنوات)", Slug = "kids-child", ParentCategoryId = kids.Id, SortOrder = 2 },
                new() { Name = "كبار (8-16 سنة)", Slug = "kids-teen", ParentCategoryId = kids.Id, SortOrder = 3 },
            });

            await context.SaveChangesAsync();
        }
    }
}
