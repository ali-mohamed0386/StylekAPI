using StylekAPI.Models.Enums;

namespace StylekAPI.DTOs.Products;

public class ProductFilterDto
{
    public int? CategoryId { get; set; }
    public Gender? Gender { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductListDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal FinalPrice => DiscountPrice ?? Price;
    public Gender Gender { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsLuxury { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsBestSeller { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string CategoryNameEn { get; set; } = string.Empty;
}

public class ProductDetailDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal FinalPrice => DiscountPrice ?? Price;
    public Gender Gender { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsLuxury { get; set; }
    public int Stock { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public CategorySummaryDto Category { get; set; } = null!;
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class CategorySummaryDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;
}
