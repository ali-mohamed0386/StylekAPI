using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.Models;
using StylekAPI.Models.Enums;

namespace StylekAPI.Services;

public class OtpService
{
    private readonly ApplicationDbContext _context;

    public OtpService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAndSaveAsync(string email, OtpPurpose purpose)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();

        var otp = new OtpCode
        {
            Email = email.ToLowerInvariant(),
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        return code;
    }

    public async Task<bool> CheckAsync(string email, string code, OtpPurpose purpose)
    {
        return await _context.OtpCodes.AnyAsync(o =>
            o.Email == email.ToLowerInvariant()
            && o.Code == code
            && o.Purpose == purpose
            && !o.IsUsed
            && o.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<bool> VerifyAndMarkUsedAsync(string email, string code, OtpPurpose purpose)
    {
        var otp = await _context.OtpCodes
            .Where(o => o.Email == email.ToLowerInvariant()
                        && o.Code == code
                        && o.Purpose == purpose
                        && !o.IsUsed
                        && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        if (otp == null) return false;

        otp.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
