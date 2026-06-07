using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Cart;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class CartService
{
    private readonly ApplicationDbContext _context;
    private static readonly ConcurrentDictionary<string, string?> AppliedCoupons = new();

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var items = await _context.CartItems
            .Include(c => c.Product).ThenInclude(p => p.Images)
            .Include(c => c.ProductVariant)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return BuildCartDto(userId, items);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive)
            ?? throw new KeyNotFoundException("Product not found.");

        ProductVariant? variant = null;
        if (dto.ProductVariantId.HasValue)
        {
            variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId && v.ProductId == dto.ProductId)
                ?? throw new KeyNotFoundException("Product variant not found.");
        }

        var stock = variant?.Stock ?? product.Stock;
        if (stock < dto.Quantity)
            throw new InvalidOperationException("Insufficient stock.");

        var existing = await _context.CartItems.FirstOrDefaultAsync(c =>
            c.UserId == userId &&
            c.ProductId == dto.ProductId &&
            c.ProductVariantId == dto.ProductVariantId);

        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
            if (existing.Quantity > stock)
                throw new InvalidOperationException("Insufficient stock.");
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity
            });
        }

        await _context.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateQuantityAsync(string userId, int cartItemId, UpdateCartQuantityDto dto)
    {
        var item = await _context.CartItems
            .Include(c => c.Product)
            .Include(c => c.ProductVariant)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        var stock = item.ProductVariant?.Stock ?? item.Product.Stock;
        if (dto.Quantity > stock)
            throw new InvalidOperationException("Insufficient stock.");

        item.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveItemAsync(string userId, int cartItemId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task ClearCartAsync(string userId)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        AppliedCoupons.TryRemove(userId, out _);
        await _context.SaveChangesAsync();
    }

    public async Task<CartDto> ApplyCouponAsync(string userId, ApplyCouponDto dto)
    {
        var cart = await GetCartAsync(userId);
        if (cart.SubTotal <= 0)
            throw new InvalidOperationException("Cart is empty.");

        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == dto.Code && c.IsActive)
            ?? throw new KeyNotFoundException("Invalid coupon code.");

        if (coupon.ExpiryDate < DateTime.UtcNow)
            throw new InvalidOperationException("Coupon has expired.");

        if (coupon.UsedCount >= coupon.MaxUses)
            throw new InvalidOperationException("Coupon usage limit reached.");

        if (cart.SubTotal < coupon.MinOrderAmount)
            throw new InvalidOperationException($"Minimum order amount is {coupon.MinOrderAmount:N2} EGP.");

        AppliedCoupons[userId] = coupon.Code;
        return await GetCartAsync(userId);
    }

    public string? GetAppliedCouponCode(string userId) =>
        AppliedCoupons.TryGetValue(userId, out var code) ? code : null;

    public void ClearAppliedCoupon(string userId) =>
        AppliedCoupons.TryRemove(userId, out _);

    private CartDto BuildCartDto(string userId, List<CartItem> items)
    {
        var cartItems = items.Select(i =>
        {
            var unitPrice = i.Product.DiscountPrice ?? i.Product.Price;
            return new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductNameEn = i.Product.NameEn,
                ProductImageUrl = i.Product.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault(img => img.IsPrimary)?.ImageUrl
                    ?? i.Product.Images.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
                ProductVariantId = i.ProductVariantId,
                Size = i.ProductVariant?.Size,
                Color = i.ProductVariant?.Color,
                Quantity = i.Quantity,
                UnitPrice = unitPrice,
                LineTotal = unitPrice * i.Quantity
            };
        }).ToList();

        var subTotal = cartItems.Sum(i => i.LineTotal);
        var discount = CalculateDiscount(userId, subTotal);

        return new CartDto
        {
            Items = cartItems,
            SubTotal = subTotal,
            DiscountAmount = discount,
            Total = subTotal - discount,
            AppliedCouponCode = GetAppliedCouponCode(userId)
        };
    }

    private decimal CalculateDiscount(string userId, decimal subTotal)
    {
        var code = GetAppliedCouponCode(userId);
        if (string.IsNullOrEmpty(code)) return 0;

        var coupon = _context.Coupons.FirstOrDefault(c => c.Code == code && c.IsActive);
        if (coupon == null) return 0;

        if (coupon.ExpiryDate < DateTime.UtcNow || coupon.UsedCount >= coupon.MaxUses || subTotal < coupon.MinOrderAmount)
            return 0;

        if (coupon.DiscountPercent.HasValue)
            return Math.Round(subTotal * coupon.DiscountPercent.Value / 100, 2);

        if (coupon.DiscountAmount.HasValue)
            return Math.Min(coupon.DiscountAmount.Value, subTotal);

        return 0;
    }
}
