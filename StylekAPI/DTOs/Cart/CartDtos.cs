namespace StylekAPI.DTOs.Cart;

public class AddCartItemDto
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartQuantityDto
{
    public int Quantity { get; set; }
}

public class ApplyCouponDto
{
    public string Code { get; set; } = string.Empty;
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? AppliedCouponCode { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNameEn { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int? ProductVariantId { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
