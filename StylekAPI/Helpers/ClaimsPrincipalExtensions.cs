using System.Security.Claims;

namespace StylekAPI.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated.");

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Admin);

    public static bool IsManager(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Manager);

    public static bool CanHardDelete(this ClaimsPrincipal user) =>
        user.IsInRole(AppRoles.Admin);
}
