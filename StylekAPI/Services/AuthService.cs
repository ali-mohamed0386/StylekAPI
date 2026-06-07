using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Auth;
using StylekAPI.Helpers;
using StylekAPI.Models;
using StylekAPI.Models.Enums;

namespace StylekAPI.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly OtpService _otpService;
    private readonly EmailService _emailService;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        JwtTokenHelper jwtTokenHelper,
        OtpService otpService,
        EmailService emailService,
        IMapper mapper)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenHelper = jwtTokenHelper;
        _otpService = otpService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email is already registered.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PreferredLanguage = dto.PreferredLanguage,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Customer");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return;

        var code = await _otpService.GenerateAndSaveAsync(dto.Email, OtpPurpose.ForgotPassword);
        await _emailService.SendOtpEmailAsync(dto.Email, code);
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
    {
        return await _otpService.CheckAsync(dto.Email, dto.Code, OtpPurpose.ForgotPassword);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var isValid = await _otpService.VerifyAndMarkUsedAsync(dto.Email, dto.Code, OtpPurpose.ForgotPassword);
        if (!isValid)
            throw new InvalidOperationException("Invalid or expired OTP code.");

        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new KeyNotFoundException("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<UserDto> GetMeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = roles.ToList();
        return dto;
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenHelper.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenHelper.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _jwtTokenHelper.GetRefreshTokenExpiry();
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                PreferredLanguage = user.PreferredLanguage,
                Roles = roles.ToList()
            }
        };
    }
}
