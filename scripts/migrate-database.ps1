# Database Migration Script
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("SqlServer", "Sqlite", "Postgres")]
    [string]$Provider = "SqlServer",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Reset
)

Write-Host "=== EduTrack Database Migration ===" -ForegroundColor Green
Write-Host "Provider: $Provider" -ForegroundColor Yellow

# Set working directory to WebApp
$webAppPath = Join-Path $PSScriptRoot "..\src\EduTrack.WebApp"
Set-Location $webAppPath

# Update appsettings.json with selected provider
$appsettingsPath = "appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $appsettings.Database.Provider = $Provider
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "Updated appsettings.json with provider: $Provider" -ForegroundColor Green
}

# Update connection string if provided
if ($ConnectionString -ne "") {
    $appsettings.ConnectionStrings.$Provider = $ConnectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "Updated connection string for $Provider" -ForegroundColor Green
}

try {
    if ($Reset) {
        Write-Host "Resetting database..." -ForegroundColor Yellow
        dotnet ef database drop --force --project ..\EduTrack.Infrastructure
    }
    
    Write-Host "Applying migrations..." -ForegroundColor Yellow
    dotnet ef database update --project ..\EduTrack.Infrastructure
    
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    
    # Run seeding
    Write-Host "Running data seeding..." -ForegroundColor Yellow
    dotnet run --no-build -- --seed
    
    Write-Host "Database setup completed!" -ForegroundColor Green
}
catch {
    Write-Host "Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "=== Migration Complete ===" -ForegroundColor Green
