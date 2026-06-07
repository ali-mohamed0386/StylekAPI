using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.DTOs.Products;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductListDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive)
            .AsQueryable();

        query = ApplyFilters(query, filter);
        query = ApplySorting(query, filter.Sort);

        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<ProductListDto>
        {
            Items = _mapper.Map<List<ProductListDto>>(products),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ProductListDto>> SearchProductsAsync(string search, int page = 1, int pageSize = 12)
    {
        return await GetProductsAsync(new ProductFilterDto
        {
            Search = search,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<List<ProductListDto>> GetFeaturedAsync(int count = 8)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ProductListDto>>(products);
    }

    public async Task<ProductDetailDto> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive)
            ?? throw new KeyNotFoundException("Product not found.");

        return _mapper.Map<ProductDetailDto>(product);
    }

    // --- Admin ---

    public async Task<PagedResult<AdminProductDto>> GetAllAdminAsync(AdminListFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        if (!filter.IncludeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(p => EF.Functions.Like(p.NameEn, term) || EF.Functions.Like(p.NameAr, term));
        }

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<AdminProductDto>
        {
            Items = products.Select(MapAdminProduct).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminProductDto> GetAdminByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Product not found.");

        return MapAdminProduct(product);
    }

    public async Task<AdminProductDto> CreateAsync(CreateProductDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");

        var product = new Product
        {
            CategoryId = dto.CategoryId,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            DescriptionEn = dto.DescriptionEn,
            DescriptionAr = dto.DescriptionAr,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            Gender = dto.Gender,
            IsFeatured = dto.IsFeatured,
            IsLuxury = dto.IsLuxury,
            IsNewArrival = dto.IsNewArrival,
            IsBestSeller = dto.IsBestSeller,
            Stock = dto.Stock,
            IsActive = dto.IsActive
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(dto.PrimaryImageUrl))
        {
            _context.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = dto.PrimaryImageUrl,
                IsPrimary = true,
                DisplayOrder = 1
            });
            await _context.SaveChangesAsync();
        }

        return await GetAdminByIdAsync(product.Id);
    }

    public async Task<AdminProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Product not found.");

        product.CategoryId = dto.CategoryId;
        product.NameEn = dto.NameEn;
        product.NameAr = dto.NameAr;
        product.DescriptionEn = dto.DescriptionEn;
        product.DescriptionAr = dto.DescriptionAr;
        product.Price = dto.Price;
        product.DiscountPrice = dto.DiscountPrice;
        product.Gender = dto.Gender;
        product.IsFeatured = dto.IsFeatured;
        product.IsLuxury = dto.IsLuxury;
        product.IsNewArrival = dto.IsNewArrival;
        product.IsBestSeller = dto.IsBestSeller;
        product.Stock = dto.Stock;
        product.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return await GetAdminByIdAsync(id);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        product.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    private static AdminProductDto MapAdminProduct(Product p) => new()
    {
        Id = p.Id,
        CategoryId = p.CategoryId,
        CategoryNameEn = p.Category?.NameEn ?? string.Empty,
        NameEn = p.NameEn,
        NameAr = p.NameAr,
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        Gender = p.Gender,
        IsFeatured = p.IsFeatured,
        IsLuxury = p.IsLuxury,
        IsNewArrival = p.IsNewArrival,
        IsBestSeller = p.IsBestSeller,
        Stock = p.Stock,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        PrimaryImageUrl = p.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl
            ?? p.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl
    };

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterDto filter)
    {
        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.Gender.HasValue)
            query = query.Where(p => p.Gender == filter.Gender.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.Price) >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.Price) <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.NameEn, term) ||
                EF.Functions.Like(p.NameAr, term) ||
                EF.Functions.Like(p.DescriptionEn, term) ||
                EF.Functions.Like(p.DescriptionAr, term));
        }

        return query;
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sort)
    {
        return sort?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            "name" => query.OrderBy(p => p.NameEn),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
