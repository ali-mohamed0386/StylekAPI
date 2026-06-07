namespace StylekAPI.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Size { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
