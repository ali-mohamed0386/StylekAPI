using System.Collections.Concurrent;

namespace StylekAPI.Middleware;

public class AuthRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> Requests = new();
    private static readonly string[] LimitedPaths =
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/forgot-password"
    };

    private const int MaxRequests = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public AuthRateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (LimitedPaths.Any(p => path.EndsWith(p)))
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"{ip}:{path}";

            var entry = Requests.GetOrAdd(key, _ => new RateLimitEntry());
            var isLimited = false;

            lock (entry)
            {
                if (DateTime.UtcNow - entry.WindowStart > Window)
                {
                    entry.WindowStart = DateTime.UtcNow;
                    entry.Count = 0;
                }

                entry.Count++;
                isLimited = entry.Count > MaxRequests;
            }

            if (isLimited)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Too many requests. Please try again later.",
                    errors = new List<string>()
                });
                return;
            }
        }

        await _next(context);
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    }
}
