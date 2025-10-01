# PowerShell script to create SQLite migration
# This script creates the initial migration for SQLite

Write-Host "Creating SQLite migration..." -ForegroundColor Green

Set-Location "src\EduTrack.Infrastructure"

# Create initial migration
dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Migrations\Sqlite

Write-Host "SQLite migration created successfully!" -ForegroundColor Green
Write-Host "To apply the migration, run: dotnet ef database update --context AppDbContext" -ForegroundColor Yellow
