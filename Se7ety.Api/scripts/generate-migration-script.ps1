param(
    [string]$Output = ".\SqlScripts\Se7ety_migrations.sql"
)

$ErrorActionPreference = "Stop"

$outputDirectory = Split-Path -Parent $Output
if ($outputDirectory -and -not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

dotnet ef migrations script --idempotent --context ApplicationDbContext --output $Output
