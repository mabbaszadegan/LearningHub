# PowerShell script to create PostgreSQL migration
# This script creates the initial migration for PostgreSQL

Write-Host "Creating PostgreSQL migration..." -ForegroundColor Green

Set-Location "src\EduTrack.Infrastructure"

# Create initial migration for PostgreSQL
dotnet ef migrations add InitialCreate_Postgres --context AppDbContext --output-dir Migrations\Postgres

Write-Host "PostgreSQL migration created successfully!" -ForegroundColor Green
Write-Host "To apply the migration, run: dotnet ef database update --context AppDbContext" -ForegroundColor Yellow
