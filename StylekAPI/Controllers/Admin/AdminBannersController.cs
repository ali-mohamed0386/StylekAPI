using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.AdminOrManager)]
[ApiController]
[Route("api/admin/banners")]
public class AdminBannersController : ControllerBase
{
    private readonly BannerService _bannerService;

    public AdminBannersController(BannerService bannerService)
    {
        _bannerService = bannerService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminBannerDto>>>> GetAll([FromQuery] bool includeInactive = true)
    {
        var banners = await _bannerService.GetAllAdminAsync(includeInactive);
        return Ok(ApiResponse<List<AdminBannerDto>>.Ok(banners));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminBannerDto>>> GetById(int id)
    {
        var banner = await _bannerService.GetAdminByIdAsync(id);
        return Ok(ApiResponse<AdminBannerDto>.Ok(banner));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminBannerDto>>> Create(CreateBannerDto dto)
    {
        var banner = await _bannerService.CreateAsync(dto);
        return Ok(ApiResponse<AdminBannerDto>.Ok(banner, "Banner created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AdminBannerDto>>> Update(int id, UpdateBannerDto dto)
    {
        var banner = await _bannerService.UpdateAsync(id, dto);
        return Ok(ApiResponse<AdminBannerDto>.Ok(banner, "Banner updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> SoftDelete(int id)
    {
        await _bannerService.SoftDeleteAsync(id);
        return Ok(ApiResponse.Ok("Banner deactivated successfully"));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(int id)
    {
        await _bannerService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("Banner permanently deleted"));
    }
}
