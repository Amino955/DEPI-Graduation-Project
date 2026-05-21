using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TellaStore.Models.Entities;

namespace TellaStore.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<DeliveryAssignment> DeliveryAssignments { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global Soft Delete Filters — automatically exclude deleted records
        builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        builder.Entity<ProductVariant>().HasQueryFilter(v => !v.IsDeleted);
        builder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        builder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
        builder.Entity<WishlistItem>().HasQueryFilter(w => !w.IsDeleted);

        // Unique Indexes
        builder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        builder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        builder.Entity<ProductVariant>().HasIndex(v => v.SKU).IsUnique();
        builder.Entity<WishlistItem>().HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
        builder.Entity<Review>().HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();

        // Cascade Delete Rules
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Variant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderStatusHistory>()
            .HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // SQL Server: avoid multiple cascade paths (Order/User → DeliveryAssignment)
        builder.Entity<DeliveryAssignment>()
            .HasOne(d => d.Order)
            .WithOne(o => o.DeliveryAssignment)
            .HasForeignKey<DeliveryAssignment>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DeliveryAssignment>()
            .HasOne(d => d.DeliveryUser)
            .WithMany()
            .HasForeignKey(d => d.DeliveryUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderStatusHistory>()
            .HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // When variant is deleted, don't delete its images — just set VariantId to null
        builder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductImage>()
            .HasOne(pi => pi.Variant)
            .WithMany(v => v.Images)
            .HasForeignKey(pi => pi.VariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<WishlistItem>()
            .HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Discount>()
            .HasOne(d => d.Product)
            .WithMany(p => p.Discounts)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Discount>()
            .HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    // Auto-update timestamps on save
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
                ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
