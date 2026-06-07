using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.DTOs.Categories;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class CategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoryService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive)
            ?? throw new KeyNotFoundException("Category not found.");

        return _mapper.Map<CategoryDto>(category);
    }

    // --- Admin ---

    public async Task<List<AdminCategoryDto>> GetAllAdminAsync(bool includeInactive = true)
    {
        var query = _context.Categories.AsQueryable();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                Slug = c.Slug,
                Gender = c.Gender,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync();

        return categories;
    }

    public async Task<AdminCategoryDto> GetAdminByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Category not found.");

        return new AdminCategoryDto
        {
            Id = category.Id,
            NameEn = category.NameEn,
            NameAr = category.NameAr,
            Slug = category.Slug,
            Gender = category.Gender,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count(p => p.IsActive)
        };
    }

    public async Task<AdminCategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Slug == dto.Slug);
        if (exists)
            throw new InvalidOperationException("Category slug already exists.");

        var category = new Category
        {
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            Slug = dto.Slug,
            Gender = dto.Gender,
            ImageUrl = dto.ImageUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return await GetAdminByIdAsync(category.Id);
    }

    public async Task<AdminCategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        var slugTaken = await _context.Categories.AnyAsync(c => c.Slug == dto.Slug && c.Id != id);
        if (slugTaken)
            throw new InvalidOperationException("Category slug already exists.");

        category.NameEn = dto.NameEn;
        category.NameAr = dto.NameAr;
        category.Slug = dto.Slug;
        category.Gender = dto.Gender;
        category.ImageUrl = dto.ImageUrl;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return await GetAdminByIdAsync(id);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        category.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete category with existing products.");

        var category = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
