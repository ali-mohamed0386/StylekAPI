using StylekAPI.Models.Enums;

namespace StylekAPI.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}
