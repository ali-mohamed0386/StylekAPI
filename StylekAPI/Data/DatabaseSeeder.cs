using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StylekAPI.Helpers;
using StylekAPI.Models;
using StylekAPI.Models.Enums;

namespace StylekAPI.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var appSettings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;

        await EnsureDatabaseAsync(context, environment);

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, appSettings);

        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { NameEn = "Men", NameAr = "رجال", Slug = "men", Gender = Gender.Man, DisplayOrder = 1, ImageUrl = "/uploads/products/seed-men.jpg" },
            new() { NameEn = "Women", NameAr = "نساء", Slug = "women", Gender = Gender.Woman, DisplayOrder = 2, ImageUrl = "/uploads/products/seed-women.jpg" },
            new() { NameEn = "Baby", NameAr = "أطفال", Slug = "baby", Gender = Gender.Baby, DisplayOrder = 3, ImageUrl = "/uploads/products/seed-baby.jpg" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            CreateProduct(categories[0].Id, "Classic White Shirt", "قميص أبيض كلاسيكي", "Premium cotton shirt for men.", "قميص قطني فاخر للرجال.", 899, 749, Gender.Man, true, false, true, false, 50),
            CreateProduct(categories[0].Id, "Slim Fit Jeans", "جينز slim fit", "Modern slim fit denim jeans.", "جينز عصري بقصة ضيقة.", 1299, null, Gender.Man, false, false, true, true, 40),
            CreateProduct(categories[0].Id, "Leather Jacket", "جاكيت جلد", "Genuine leather jacket.", "جاكيت جلد طبيعي.", 3499, 2999, Gender.Man, true, true, false, false, 15),
            CreateProduct(categories[1].Id, "Floral Summer Dress", "فستان صيفي floral", "Light floral dress for summer.", "فستان خفيف بنقشة زهرية.", 1199, 999, Gender.Woman, true, false, true, true, 35),
            CreateProduct(categories[1].Id, "Silk Blouse", "بلوزة حرير", "Elegant silk blouse.", "بلوزة حرير أنيقة.", 1599, null, Gender.Woman, false, true, false, false, 25),
            CreateProduct(categories[1].Id, "High Heel Shoes", "حذاء كعب عالي", "Stylish high heel shoes.", "حذاء كعب عالي أنيق.", 1899, 1499, Gender.Woman, true, true, false, true, 20),
            CreateProduct(categories[2].Id, "Baby Cotton Set", "طقم قطن للأطفال", "Soft cotton set for babies.", "طقم قطن ناعم للأطفال.", 599, null, Gender.Baby, false, false, true, false, 60),
            CreateProduct(categories[2].Id, "Baby Sneakers", "حذاء رياضي أطفال", "Comfortable baby sneakers.", "حذاء رياضي مريح للأطفال.", 699, 549, Gender.Baby, true, false, true, false, 45),
            CreateProduct(categories[2].Id, "Knitted Baby Blanket", "بطانية أطفال", "Warm knitted blanket.", "بطانية محبوكة دافئة.", 449, null, Gender.Baby, false, false, false, true, 30),
            CreateProduct(categories[1].Id, "Designer Handbag", "حقيبة يد فاخرة", "Luxury designer handbag.", "حقيبة يد فاخرة.", 4999, 4299, Gender.Woman, true, true, false, true, 10)
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        foreach (var product in products)
        {
            context.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = $"/uploads/products/product-{product.Id}.jpg",
                IsPrimary = true,
                DisplayOrder = 1
            });

            if (product.Id <= 4)
            {
                context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    Size = "M",
                    Color = "Black",
                    Stock = 10,
                    Sku = $"SK-{product.Id}-M-BLK"
                });
                context.ProductVariants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    Size = "L",
                    Color = "White",
                    Stock = 8,
                    Sku = $"SK-{product.Id}-L-WHT"
                });
            }
        }

        context.Banners.AddRange(
            new Banner { TitleEn = "Summer Sale", TitleAr = "تخفيضات الصيف", ImageUrl = "/uploads/banners/banner1.jpg", LinkUrl = "/offers", DisplayOrder = 1 },
            new Banner { TitleEn = "New Arrivals", TitleAr = "وصل حديثاً", ImageUrl = "/uploads/banners/banner2.jpg", LinkUrl = "/new", DisplayOrder = 2 },
            new Banner { TitleEn = "Luxury Collection", TitleAr = "مجموعة فاخرة", ImageUrl = "/uploads/banners/banner3.jpg", LinkUrl = "/luxury", DisplayOrder = 3 }
        );

        context.Coupons.Add(new Coupon
        {
            Code = "STYLEK10",
            DiscountPercent = 10,
            MinOrderAmount = 500,
            ExpiryDate = DateTime.UtcNow.AddMonths(6),
            MaxUses = 1000,
            UsedCount = 0,
            IsActive = true
        });

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDatabaseAsync(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        var migrations = context.Database.GetMigrations().ToList();

        if (migrations.Count == 0)
        {
            if (!await context.Database.CanConnectAsync())
                await context.Database.EnsureCreatedAsync();
            return;
        }

        var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
        var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

        if (applied.Count == 0 && await TableExistsAsync(context, "AspNetRoles"))
        {
            if (environment.IsDevelopment())
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.MigrateAsync();
            }
            else
            {
                throw new InvalidOperationException(
                    "Database exists without migration history. Run 'Update-Database' in Package Manager Console or apply migrations manually.");
            }
            return;
        }

        if (pending.Count > 0)
            await context.Database.MigrateAsync();
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext context, string tableName)
    {
        if (!await context.Database.CanConnectAsync())
            return false;

        await context.Database.OpenConnectionAsync();
        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT CASE WHEN OBJECT_ID(@table, 'U') IS NOT NULL THEN 1 ELSE 0 END";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@table";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) == 1;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Manager", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, AppSettings settings)
    {
        var admin = await userManager.FindByEmailAsync(settings.AdminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = settings.AdminEmail,
                Email = settings.AdminEmail,
                FullName = "Stylek Admin",
                EmailConfirmed = true,
                PreferredLanguage = "en"
            };

            var result = await userManager.CreateAsync(admin, settings.AdminPassword);
            if (!result.Succeeded) return;
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static Product CreateProduct(
        int categoryId, string nameEn, string nameAr, string descEn, string descAr,
        decimal price, decimal? discount, Gender gender,
        bool featured, bool luxury, bool newArrival, bool bestSeller, int stock)
    {
        return new Product
        {
            CategoryId = categoryId,
            NameEn = nameEn,
            NameAr = nameAr,
            DescriptionEn = descEn,
            DescriptionAr = descAr,
            Price = price,
            DiscountPrice = discount,
            Gender = gender,
            IsFeatured = featured,
            IsLuxury = luxury,
            IsNewArrival = newArrival,
            IsBestSeller = bestSeller,
            Stock = stock
        };
    }
}
