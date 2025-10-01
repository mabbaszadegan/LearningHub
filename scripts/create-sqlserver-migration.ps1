# PowerShell script to create SQL Server migration
# This script creates the initial migration for SQL Server

Write-Host "Creating SQL Server migration..." -ForegroundColor Green

Set-Location "src\EduTrack.Infrastructure"

# Create initial migration for SQL Server
dotnet ef migrations add InitialCreate_SqlServer --context AppDbContext --output-dir Migrations\SqlServer

Write-Host "SQL Server migration created successfully!" -ForegroundColor Green
Write-Host "To apply the migration, run: dotnet ef database update --context AppDbContext" -ForegroundColor Yellow
