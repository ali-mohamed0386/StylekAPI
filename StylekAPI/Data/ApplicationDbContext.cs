using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Models;

namespace StylekAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Banner> Banners => Set<Banner>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.NameEn).HasMaxLength(200);
            e.Property(c => c.NameAr).HasMaxLength(200);
        });

        builder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.DiscountPrice).HasPrecision(18, 2);
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductImage>(e =>
        {
            e.HasOne(pi => pi.Product).WithMany(p => p.Images).HasForeignKey(pi => pi.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVariant>(e =>
        {
            e.HasOne(pv => pv.Product).WithMany(p => p.Variants).HasForeignKey(pv => pv.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(pv => pv.Sku).IsUnique();
        });

        builder.Entity<CartItem>(e =>
        {
            e.HasIndex(c => new { c.UserId, c.ProductId, c.ProductVariantId }).IsUnique();
            e.HasOne(c => c.User).WithMany(u => u.CartItems).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.ProductVariant).WithMany().HasForeignKey(c => c.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WishlistItem>(e =>
        {
            e.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            e.HasOne(w => w.User).WithMany(u => u.WishlistItems).HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Order>(e =>
        {
            e.Property(o => o.SubTotal).HasPrecision(18, 2);
            e.Property(o => o.DiscountAmount).HasPrecision(18, 2);
            e.Property(o => o.ShippingFee).HasPrecision(18, 2);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Coupon).WithMany(c => c.Orders).HasForeignKey(o => o.CouponId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            e.HasOne(oi => oi.Order).WithMany(o => o.OrderItems).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Coupon>(e =>
        {
            e.Property(c => c.DiscountPercent).HasPrecision(18, 2);
            e.Property(c => c.DiscountAmount).HasPrecision(18, 2);
            e.Property(c => c.MinOrderAmount).HasPrecision(18, 2);
            e.HasIndex(c => c.Code).IsUnique();
        });

        builder.Entity<Review>(e =>
        {
            e.HasIndex(r => new { r.UserId, r.ProductId }).IsUnique();
            e.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReviewLike>(e =>
        {
            e.HasIndex(rl => new { rl.UserId, rl.ReviewId }).IsUnique();
            e.HasOne(rl => rl.User).WithMany(u => u.ReviewLikes).HasForeignKey(rl => rl.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(rl => rl.Review).WithMany(r => r.Likes).HasForeignKey(rl => rl.ReviewId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OtpCode>(e =>
        {
            e.HasIndex(o => new { o.Email, o.Purpose, o.IsUsed });
        });
    }
}
