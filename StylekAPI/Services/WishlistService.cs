using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class WishlistService
{
    private readonly ApplicationDbContext _context;

    public WishlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(string userId, int productId)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId && p.IsActive);
        if (!productExists)
            throw new KeyNotFoundException("Product not found.");

        var exists = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

        if (exists)
            throw new InvalidOperationException("Product is already in wishlist.");

        _context.WishlistItems.Add(new WishlistItem
        {
            UserId = userId,
            ProductId = productId
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId)
            ?? throw new KeyNotFoundException("Wishlist item not found.");

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CheckAsync(string userId, int productId)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }
}
