using StylekAPI.DTOs.Categories;
using StylekAPI.DTOs.Products;

namespace StylekAPI.DTOs.Home;

public class HomePageDto
{
    public List<BannerDto> Banners { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ProductListDto> NewArrivals { get; set; } = new();
    public List<ProductListDto> BestSellers { get; set; } = new();
    public List<ProductListDto> LuxuryPicks { get; set; } = new();
    public List<ProductListDto> Offers { get; set; } = new();
}

public class BannerDto
{
    public int Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
}
