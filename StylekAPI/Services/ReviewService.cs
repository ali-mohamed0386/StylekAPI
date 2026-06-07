using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.DTOs.Reviews;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class ReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReviewService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReviewDto> AddReviewAsync(string userId, CreateReviewDto dto)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId && p.IsActive);
        if (!productExists)
            throw new KeyNotFoundException("Product not found.");

        var existing = await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == dto.ProductId && r.IsActive);

        if (existing)
            throw new InvalidOperationException("You have already reviewed this product.");

        var review = new Review
        {
            UserId = userId,
            ProductId = dto.ProductId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await MapReviewAsync(review.Id, userId);
    }

    public async Task<ReviewDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto dto)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && r.IsActive)
            ?? throw new KeyNotFoundException("Review not found.");

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        await _context.SaveChangesAsync();

        return await MapReviewAsync(review.Id, userId);
    }

    public async Task DeleteReviewAsync(string userId, int reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && r.IsActive)
            ?? throw new KeyNotFoundException("Review not found.");

        review.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task<ReviewDto> ToggleLikeAsync(string userId, int reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.IsActive)
            ?? throw new KeyNotFoundException("Review not found.");

        var existingLike = await _context.ReviewLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ReviewId == reviewId);

        if (existingLike != null)
        {
            _context.ReviewLikes.Remove(existingLike);
            review.LikesCount = Math.Max(0, review.LikesCount - 1);
        }
        else
        {
            _context.ReviewLikes.Add(new ReviewLike { UserId = userId, ReviewId = reviewId });
            review.LikesCount++;
        }

        await _context.SaveChangesAsync();
        return await MapReviewAsync(reviewId, userId);
    }

    public async Task<List<ReviewDto>> GetProductReviewsAsync(int productId, string? userId = null)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var result = new List<ReviewDto>();
        foreach (var review in reviews)
        {
            var dto = _mapper.Map<ReviewDto>(review);
            if (userId != null)
            {
                dto.IsLikedByCurrentUser = await _context.ReviewLikes
                    .AnyAsync(l => l.UserId == userId && l.ReviewId == review.Id);
            }
            result.Add(dto);
        }

        return result;
    }

    private async Task<ReviewDto> MapReviewAsync(int reviewId, string userId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstAsync(r => r.Id == reviewId);

        var dto = _mapper.Map<ReviewDto>(review);
        dto.IsLikedByCurrentUser = await _context.ReviewLikes
            .AnyAsync(l => l.UserId == userId && l.ReviewId == reviewId);
        return dto;
    }

    // --- Admin ---

    public async Task<PagedResult<AdminReviewDto>> GetAllAdminAsync(AdminListFilterDto filter)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsQueryable();

        if (!filter.IncludeInactive)
            query = query.Where(r => r.IsActive);

        var totalCount = await query.CountAsync();
        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<AdminReviewDto>
        {
            Items = reviews.Select(MapAdminReview).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminReviewDto> GetAdminByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Review not found.");

        return MapAdminReview(review);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id)
            ?? throw new KeyNotFoundException("Review not found.");

        review.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Review not found.");

        _context.ReviewLikes.RemoveRange(review.Likes);
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    private static AdminReviewDto MapAdminReview(Review r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        ProductName = r.Product?.NameEn ?? string.Empty,
        UserName = r.User?.FullName ?? string.Empty,
        UserEmail = r.User?.Email ?? string.Empty,
        Rating = r.Rating,
        Comment = r.Comment,
        LikesCount = r.LikesCount,
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt
    };
}
