using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class CouponService
{
    private readonly ApplicationDbContext _context;

    public CouponService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminCouponDto>> GetAllAdminAsync(AdminListFilterDto filter)
    {
        var query = _context.Coupons.AsQueryable();

        if (!filter.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(c => EF.Functions.Like(c.Code, term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => Map(c))
            .ToListAsync();

        return new PagedResult<AdminCouponDto>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminCouponDto> GetAdminByIdAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");

        return Map(coupon);
    }

    public async Task<AdminCouponDto> CreateAsync(CreateCouponDto dto)
    {
        var exists = await _context.Coupons.AnyAsync(c => c.Code == dto.Code);
        if (exists)
            throw new InvalidOperationException("Coupon code already exists.");

        var coupon = new Coupon
        {
            Code = dto.Code.ToUpperInvariant(),
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = dto.DiscountAmount,
            MinOrderAmount = dto.MinOrderAmount,
            ExpiryDate = dto.ExpiryDate,
            MaxUses = dto.MaxUses,
            IsActive = dto.IsActive
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        return Map(coupon);
    }

    public async Task<AdminCouponDto> UpdateAsync(int id, UpdateCouponDto dto)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");

        var codeTaken = await _context.Coupons.AnyAsync(c => c.Code == dto.Code && c.Id != id);
        if (codeTaken)
            throw new InvalidOperationException("Coupon code already exists.");

        coupon.Code = dto.Code.ToUpperInvariant();
        coupon.DiscountPercent = dto.DiscountPercent;
        coupon.DiscountAmount = dto.DiscountAmount;
        coupon.MinOrderAmount = dto.MinOrderAmount;
        coupon.ExpiryDate = dto.ExpiryDate;
        coupon.MaxUses = dto.MaxUses;
        coupon.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return Map(coupon);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");

        coupon.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
    }

    private static AdminCouponDto Map(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        DiscountPercent = c.DiscountPercent,
        DiscountAmount = c.DiscountAmount,
        MinOrderAmount = c.MinOrderAmount,
        ExpiryDate = c.ExpiryDate,
        MaxUses = c.MaxUses,
        UsedCount = c.UsedCount,
        IsActive = c.IsActive
    };
}
