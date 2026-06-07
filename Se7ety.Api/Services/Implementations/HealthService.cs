using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Se7ety.Api.Data;
using Se7ety.Api.DTOs.Common;
using Se7ety.Api.Services.Interfaces;

namespace Se7ety.Api.Services.Implementations;

public sealed class HealthService(
    IHostEnvironment environment,
    ApplicationDbContext dbContext,
    ILogger<HealthService> logger) : IHealthService
{
    public HealthCheckResponse GetStatus()
    {
        return new HealthCheckResponse("Healthy", environment.EnvironmentName, DateTime.UtcNow);
    }

    public async Task<DatabaseHealthCheckResponse> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(dbContext.Database.GetConnectionString())
            {
                ConnectTimeout = 5
            };

            await using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            return new DatabaseHealthCheckResponse("Healthy", true, environment.EnvironmentName, DateTime.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Database health check failed.");

            return new DatabaseHealthCheckResponse(
                "Unhealthy",
                false,
                environment.EnvironmentName,
                DateTime.UtcNow,
                "Database connection failed. Check application logs for details.");
        }
    }
}
