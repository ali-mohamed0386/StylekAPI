using Microsoft.EntityFrameworkCore;

namespace Se7ety.Api.Data;

public static class ApplicationDbContextConfiguration
{
    public static void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlServer(connectionString, sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);

            sqlServerOptions.CommandTimeout(60);
        });
    }
}
