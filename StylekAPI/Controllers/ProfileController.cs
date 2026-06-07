using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Profile;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetProfile()
    {
        var profile = await _profileService.GetProfileAsync(User.GetUserId());
        return Ok(ApiResponse<ProfileDto>.Ok(profile));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile(UpdateProfileDto dto)
    {
        var profile = await _profileService.UpdateProfileAsync(User.GetUserId(), dto);
        return Ok(ApiResponse<ProfileDto>.Ok(profile, "Profile updated successfully"));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordDto dto)
    {
        await _profileService.ChangePasswordAsync(User.GetUserId(), dto);
        return Ok(ApiResponse.Ok("Password changed successfully"));
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UploadAvatar(IFormFile file)
    {
        var profile = await _profileService.UploadAvatarAsync(User.GetUserId(), file);
        return Ok(ApiResponse<ProfileDto>.Ok(profile, "Avatar uploaded successfully"));
    }

    [HttpPost("deactivate")]
    public async Task<ActionResult<ApiResponse>> DeactivateAccount()
    {
        await _profileService.DeactivateAccountAsync(User.GetUserId());
        return Ok(ApiResponse.Ok("Account deactivated successfully"));
    }
}
