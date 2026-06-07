using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class BannerService
{
    private readonly ApplicationDbContext _context;

    public BannerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminBannerDto>> GetAllAdminAsync(bool includeInactive = true)
    {
        var query = _context.Banners.AsQueryable();
        if (!includeInactive)
            query = query.Where(b => b.IsActive);

        return await query
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new AdminBannerDto
            {
                Id = b.Id,
                TitleEn = b.TitleEn,
                TitleAr = b.TitleAr,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                DisplayOrder = b.DisplayOrder,
                IsActive = b.IsActive
            })
            .ToListAsync();
    }

    public async Task<AdminBannerDto> GetAdminByIdAsync(int id)
    {
        var banner = await _context.Banners.FindAsync(id)
            ?? throw new KeyNotFoundException("Banner not found.");

        return Map(banner);
    }

    public async Task<AdminBannerDto> CreateAsync(CreateBannerDto dto)
    {
        var banner = new Banner
        {
            TitleEn = dto.TitleEn,
            TitleAr = dto.TitleAr,
            ImageUrl = dto.ImageUrl,
            LinkUrl = dto.LinkUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.Banners.Add(banner);
        await _context.SaveChangesAsync();
        return Map(banner);
    }

    public async Task<AdminBannerDto> UpdateAsync(int id, UpdateBannerDto dto)
    {
        var banner = await _context.Banners.FindAsync(id)
            ?? throw new KeyNotFoundException("Banner not found.");

        banner.TitleEn = dto.TitleEn;
        banner.TitleAr = dto.TitleAr;
        banner.ImageUrl = dto.ImageUrl;
        banner.LinkUrl = dto.LinkUrl;
        banner.DisplayOrder = dto.DisplayOrder;
        banner.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return Map(banner);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var banner = await _context.Banners.FindAsync(id)
            ?? throw new KeyNotFoundException("Banner not found.");

        banner.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        var banner = await _context.Banners.FindAsync(id)
            ?? throw new KeyNotFoundException("Banner not found.");

        _context.Banners.Remove(banner);
        await _context.SaveChangesAsync();
    }

    private static AdminBannerDto Map(Banner b) => new()
    {
        Id = b.Id,
        TitleEn = b.TitleEn,
        TitleAr = b.TitleAr,
        ImageUrl = b.ImageUrl,
        LinkUrl = b.LinkUrl,
        DisplayOrder = b.DisplayOrder,
        IsActive = b.IsActive
    };
}
