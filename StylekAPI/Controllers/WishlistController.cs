using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly WishlistService _wishlistService;

    public WishlistController(WishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpPost("{productId:int}")]
    public async Task<ActionResult<ApiResponse>> Add(int productId)
    {
        await _wishlistService.AddAsync(User.GetUserId(), productId);
        return Ok(ApiResponse.Ok("Product added to wishlist"));
    }

    [HttpDelete("{productId:int}")]
    public async Task<ActionResult<ApiResponse>> Remove(int productId)
    {
        await _wishlistService.RemoveAsync(User.GetUserId(), productId);
        return Ok(ApiResponse.Ok("Product removed from wishlist"));
    }

    [HttpGet("check/{productId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Check(int productId)
    {
        var isInWishlist = await _wishlistService.CheckAsync(User.GetUserId(), productId);
        return Ok(ApiResponse<bool>.Ok(isInWishlist));
    }
}
