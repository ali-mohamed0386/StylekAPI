# Database Deployment

This project must not run `Update-Database`, `Drop-Database`, `EnsureCreated`, or `Database.Migrate()` against the hosted DatabaseASP SQL Server.

Use EF Core migrations only to generate SQL locally, then apply the SQL script manually to the hosted database.

## Runtime Connection

The application reads the runtime connection string from:

```text
ConnectionStrings:DefaultConnection
```

Configuration order is:

1. `appsettings.json`
2. `appsettings.{ASPNETCORE_ENVIRONMENT}.json`
3. Environment variables

For production hosting, you can override the file-based connection string with:

```text
ConnectionStrings__DefaultConnection
```

On MonsterASP.NET, set production values in:

```text
Control Panel -> Websites -> Manage website -> Scripting -> Environment Variables
```

Recommended production variables:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=tcp:{server},1433;Initial Catalog={database};User ID={user};Password={password};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30
Jwt__Issuer=Se7ety.Api
Jwt__Audience=Se7ety.Mobile
Jwt__Key={strong-production-secret-at-least-32-bytes}
Jwt__ExpiresInMinutes=60
```

Restart the AppPool after changing environment variables.

## Development LocalDB

Local development uses `appsettings.Development.json`, which overrides the production connection string with LocalDB when:

```text
ASPNETCORE_ENVIRONMENT=Development
```

Do not set `ASPNETCORE_ENVIRONMENT=Development` on production hosting. If this value is set in MonsterASP.NET, the hosted app may load the LocalDB connection string from `appsettings.Development.json`, which is not available on shared hosting.

## Production Debugging Checklist

Use these checks in this order after deployment:

1. Open `/api/health`.
   - If it returns `Healthy`, the ASP.NET Core process is running.
   - If it fails, check startup logs before debugging SQL.
2. Open `/api/health/database`.
   - If it returns `Healthy`, MonsterASP.NET can reach DatabaseASP and the connection string is usable.
   - If it returns `503`, inspect logs for the exact SQL/network/authentication exception.
3. In MonsterASP.NET Control Panel, open website Logs first.
4. If Logs do not show enough detail, enable ASP.NET Core Debug logs:
   - Go to `Detailed Settings` for the website.
   - Open `Logs`.
   - In `AspNetCore logs`, enable `Debug logs`.
   - Save and restart the AppPool.
   - Open `Files -> WebFTP`.
   - Read log files under `/wwwroot/logs/`.
   - Turn Debug logs off after troubleshooting.

Common production failure signals:

```text
Connection string 'DefaultConnection' is not configured.
```

The production connection string is missing or the environment variable name is wrong.

```text
JWT issuer, audience, and key must be configured.
```

One or more `Jwt__...` production variables are missing.

```text
Cannot open server / network-related or instance-specific error / timeout
```

The hosting server cannot reach SQL Server, port 1433 is blocked, the database host is wrong, or the database provider blocks the source network.

```text
Login failed for user
```

The SQL username/password/database mapping is wrong.

## Generate Migration SQL

Install or restore the EF CLI tool first if needed:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.7
```

Generate an idempotent script:

```powershell
dotnet ef migrations script --idempotent --context ApplicationDbContext --output .\SqlScripts\Se7ety_migrations.sql
```

`--idempotent` is recommended for shared hosting because the script checks `__EFMigrationsHistory` before applying each migration.

## Apply Script Manually

Use the DatabaseASP SQL query tool, SSMS, or Azure Data Studio:

1. Connect to `db52518.public.databaseasp.net,1433`.
2. Use SQL Server Authentication.
3. Select database `db52518`.
4. Open `SqlScripts\Se7ety_migrations.sql`.
5. Review the script.
6. Execute it once.
7. Confirm that `__EFMigrationsHistory` contains the applied migration.
8. Confirm the expected tables exist.

Do not run `Update-Database` or `Drop-Database` against the hosted database.
