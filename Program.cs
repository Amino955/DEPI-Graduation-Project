using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using TellaStore.Data;
using TellaStore.Data.Seed;
using TellaStore.Models.Entities;
using TellaStore.Services;
using TellaStore.Services.Interfaces;
using TellaStore.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// IEmailSender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Identity login/logout paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/access-denied";
});

// Session (for shopping cart storage)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".TellaStore.Session";
});

// Settings from appsettings.json
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// HttpContextAccessor — REQUIRED by CartService to access Session
builder.Services.AddHttpContextAccessor();

// Custom Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();

// Rate Limiting — protect login endpoint from brute force
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// Antiforgery for AJAX CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Middleware Pipeline — ORDER MATTERS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();           // Must be BEFORE UseAuthentication
app.UseAuthentication();
app.UseAuthorization();

// Route Configuration
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AdminDashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Run database seeder BEFORE starting the app
await DbSeeder.SeedAsync(app.Services);

// Provide a lightweight placeholder image for missing product images
app.MapGet("/images/no-image.png", async context =>
{
    // 1x1 PNG (placeholder)
    var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";
    var bytes = Convert.FromBase64String(pngBase64);
    context.Response.ContentType = "image/png";
    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
});

app.Run();
