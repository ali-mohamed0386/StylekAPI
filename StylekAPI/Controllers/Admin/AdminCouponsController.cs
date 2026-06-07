using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrManager)]
[ApiController]
[Route("api/admin/coupons")]
public class AdminCouponsController : ControllerBase
{
    private readonly CouponService _couponService;

    public AdminCouponsController(CouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminCouponDto>>>> GetAll([FromQuery] AdminListFilterDto filter)
    {
        var result = await _couponService.GetAllAdminAsync(filter);
        return Ok(ApiResponse<PagedResult<AdminCouponDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminCouponDto>>> GetById(int id)
    {
        var coupon = await _couponService.GetAdminByIdAsync(id);
        return Ok(ApiResponse<AdminCouponDto>.Ok(coupon));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminCouponDto>>> Create(CreateCouponDto dto)
    {
        var coupon = await _couponService.CreateAsync(dto);
        return Ok(ApiResponse<AdminCouponDto>.Ok(coupon, "Coupon created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminCouponDto>>> Update(int id, UpdateCouponDto dto)
    {
        var coupon = await _couponService.UpdateAsync(id, dto);
        return Ok(ApiResponse<AdminCouponDto>.Ok(coupon, "Coupon updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> SoftDelete(int id)
    {
        await _couponService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Coupon deactivated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(int id)
    {
        await _couponService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("Coupon permanently deleted"));
    }
}
