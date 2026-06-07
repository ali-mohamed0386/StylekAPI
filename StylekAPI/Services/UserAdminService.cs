using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class UserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserAdminService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<PagedResult<AdminUserDto>> GetAllAsync(AdminListFilterDto filter)
    {
        var query = _context.Users.AsQueryable();

        if (!filter.IncludeInactive)
            query = query.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.Like(u.FullName, term) ||
                (u.Email != null && EF.Functions.Like(u.Email, term)));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var items = new List<AdminUserDto>();
        foreach (var user in users)
        {
            items.Add(await MapUserAsync(user));
        }

        return new PagedResult<AdminUserDto>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminUserDto> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        return await MapUserAsync(user);
    }

    public async Task<AdminUserDto> UpdateRolesAsync(string id, UpdateUserRolesDto dto)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, dto.Roles);

        return await MapUserAsync(user);
    }

    public async Task<AdminUserDto> UpdateStatusAsync(string id, UpdateUserStatusDto dto)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        user.IsActive = dto.IsActive;
        if (!dto.IsActive)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
        }

        await _userManager.UpdateAsync(user);
        return await MapUserAsync(user);
    }

    public async Task HardDeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        var isAdmin = await _userManager.IsInRoleAsync(user, AppRoles.Admin);
        if (isAdmin)
            throw new InvalidOperationException("Cannot permanently delete an Admin user.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private async Task<AdminUserDto> MapUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new AdminUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        };
    }
}
