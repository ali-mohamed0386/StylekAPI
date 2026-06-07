using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrManager)]
[ApiController]
[Route("api/admin/reviews")]
public class AdminReviewsController : ControllerBase
{
    private readonly ReviewService _reviewService;

    public AdminReviewsController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminReviewDto>>>> GetAll([FromQuery] AdminListFilterDto filter)
    {
        var result = await _reviewService.GetAllAdminAsync(filter);
        return Ok(ApiResponse<PagedResult<AdminReviewDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminReviewDto>>> GetById(int id)
    {
        var review = await _reviewService.GetAdminByIdAsync(id);
        return Ok(ApiResponse<AdminReviewDto>.Ok(review));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> SoftDelete(int id)
    {
        await _reviewService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Review deactivated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(int id)
    {
        await _reviewService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("Review permanently deleted"));
    }
}
