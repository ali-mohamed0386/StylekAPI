using Microsoft.EntityFrameworkCore;
using Se7ety.Api.DTOs.Common;
using Se7ety.Api.Exceptions;
using System.Data.Common;

namespace Se7ety.Api.Middleware;

public sealed class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException exception)
        {
            await WriteErrorAsync(context, exception.StatusCode, exception.Message, exception.Errors);
        }
        catch (DbUpdateException exception)
        {
            if (exception.InnerException is DbException)
            {
                logger.LogWarning(exception, "Database connection failed during update.");
                await WriteErrorAsync(context, StatusCodes.Status503ServiceUnavailable, "Database service is unavailable.");
                return;
            }

            logger.LogWarning(exception, "Database update failed.");
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, "Database update failed. The data may already exist or violate a constraint.");
        }
        catch (DbException exception)
        {
            logger.LogWarning(exception, "Database connection failed.");
            await WriteErrorAsync(context, StatusCodes.Status503ServiceUnavailable, "Database service is unavailable.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception.");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            statusCode,
            message,
            context.TraceIdentifier,
            errors);

        await context.Response.WriteAsJsonAsync(response);
    }
}
