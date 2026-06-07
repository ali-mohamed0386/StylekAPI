using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Home;

namespace StylekAPI.Services;

public class HomeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public HomeService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<HomePageDto> GetHomePageAsync()
    {
        var banners = await _context.Banners
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();

        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var newArrivals = await GetProductsQuery(p => p.IsNewArrival, 8);
        var bestSellers = await GetProductsQuery(p => p.IsBestSeller, 8);
        var luxuryPicks = await GetProductsQuery(p => p.IsLuxury, 8);
        var offers = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive && p.DiscountPrice != null && p.DiscountPrice < p.Price)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        return new HomePageDto
        {
            Banners = _mapper.Map<List<BannerDto>>(banners),
            Categories = _mapper.Map<List<DTOs.Categories.CategoryDto>>(categories),
            NewArrivals = _mapper.Map<List<DTOs.Products.ProductListDto>>(newArrivals),
            BestSellers = _mapper.Map<List<DTOs.Products.ProductListDto>>(bestSellers),
            LuxuryPicks = _mapper.Map<List<DTOs.Products.ProductListDto>>(luxuryPicks),
            Offers = _mapper.Map<List<DTOs.Products.ProductListDto>>(offers)
        };
    }

    private async Task<List<Models.Product>> GetProductsQuery(
        System.Linq.Expressions.Expression<Func<Models.Product, bool>> predicate,
        int count)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive)
            .Where(predicate)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
