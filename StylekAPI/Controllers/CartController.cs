using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Cart;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
    {
        var cart = await _cartService.GetCartAsync(User.GetUserId());
        return Ok(ApiResponse<CartDto>.Ok(cart));
    }

    [HttpPost("add")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddItem(AddCartItemDto dto)
    {
        var cart = await _cartService.AddItemAsync(User.GetUserId(), dto);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Item added to cart"));
    }

    [HttpPut("{cartItemId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateQuantity(int cartItemId, UpdateCartQuantityDto dto)
    {
        var cart = await _cartService.UpdateQuantityAsync(User.GetUserId(), cartItemId, dto);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart updated"));
    }

    [HttpDelete("{cartItemId:int}")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveItem(int cartItemId)
    {
        var cart = await _cartService.RemoveItemAsync(User.GetUserId(), cartItemId);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Item removed from cart"));
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<ApiResponse>> ClearCart()
    {
        await _cartService.ClearCartAsync(User.GetUserId());
        return Ok(ApiResponse.Ok("Cart cleared"));
    }

    [HttpPost("apply-coupon")]
    public async Task<ActionResult<ApiResponse<CartDto>>> ApplyCoupon(ApplyCouponDto dto)
    {
        var cart = await _cartService.ApplyCouponAsync(User.GetUserId(), dto);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Coupon applied successfully"));
    }
}
