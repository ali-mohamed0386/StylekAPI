using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Admin;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers.Admin;

[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly UserAdminService _userAdminService;

    public AdminUsersController(UserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserDto>>>> GetAll([FromQuery] AdminListFilterDto filter)
    {
        var result = await _userAdminService.GetAllAsync(filter);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetById(string id)
    {
        var user = await _userAdminService.GetByIdAsync(id);
        return Ok(ApiResponse<AdminUserDto>.Ok(user));
    }

    [HttpPut("{id}/roles")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateRoles(string id, UpdateUserRolesDto dto)
    {
        var user = await _userAdminService.UpdateRolesAsync(id, dto);
        return Ok(ApiResponse<AdminUserDto>.Ok(user, "User roles updated"));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateStatus(string id, UpdateUserStatusDto dto)
    {
        var user = await _userAdminService.UpdateStatusAsync(id, dto);
        return Ok(ApiResponse<AdminUserDto>.Ok(user, "User status updated"));
    }

    [HttpDelete("{id}/permanent")]
    public async Task<ActionResult<ApiResponse>> HardDelete(string id)
    {
        await _userAdminService.HardDeleteAsync(id);
        return Ok(ApiResponse.Ok("User permanently deleted"));
    }
}
