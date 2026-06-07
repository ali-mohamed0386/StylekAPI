using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StylekAPI.DTOs.Profile;
using StylekAPI.Helpers;
using StylekAPI.Models;

namespace StylekAPI.Services;

public class ProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FileUploadHelper _fileUploadHelper;
    private readonly IMapper _mapper;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        FileUploadHelper fileUploadHelper,
        IMapper mapper)
    {
        _userManager = userManager;
        _fileUploadHelper = fileUploadHelper;
        _mapper = mapper;
    }

    public async Task<ProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return _mapper.Map<ProfileDto>(user);
    }

    public async Task<ProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.PreferredLanguage = dto.PreferredLanguage;

        await _userManager.UpdateAsync(user);
        return _mapper.Map<ProfileDto>(user);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<ProfileDto> UploadAvatarAsync(string userId, IFormFile file)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.AvatarUrl = await _fileUploadHelper.SaveImageAsync(file, "avatars");
        await _userManager.UpdateAsync(user);

        return _mapper.Map<ProfileDto>(user);
    }

    public async Task DeactivateAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.IsActive = false;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }
}
