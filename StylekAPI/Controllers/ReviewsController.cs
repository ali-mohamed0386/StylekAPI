using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Reviews;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewService _reviewService;

    public ReviewsController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> AddReview(CreateReviewDto dto)
    {
        var review = await _reviewService.AddReviewAsync(User.GetUserId(), dto);
        return Ok(ApiResponse<ReviewDto>.Ok(review, "Review added successfully"));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> EditReview(int id, UpdateReviewDto dto)
    {
        var review = await _reviewService.UpdateReviewAsync(User.GetUserId(), id, dto);
        return Ok(ApiResponse<ReviewDto>.Ok(review, "Review updated successfully"));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteReview(int id)
    {
        await _reviewService.DeleteReviewAsync(User.GetUserId(), id);
        return Ok(ApiResponse.Ok("Review deleted successfully"));
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> LikeReview(int id)
    {
        var review = await _reviewService.ToggleLikeAsync(User.GetUserId(), id);
        return Ok(ApiResponse<ReviewDto>.Ok(review));
    }

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetProductReviews(int productId)
    {
        string? userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;
        var reviews = await _reviewService.GetProductReviewsAsync(productId, userId);
        return Ok(ApiResponse<List<ReviewDto>>.Ok(reviews));
    }
}
