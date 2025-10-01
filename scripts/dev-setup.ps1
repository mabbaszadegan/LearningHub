# Development Setup Script for Windows PowerShell
# This script sets up the development environment for EduTrack

Write-Host "Setting up EduTrack development environment..." -ForegroundColor Green

# Check if .NET 8 is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET version: $dotnetVersion" -ForegroundColor Green
    
    if (-not $dotnetVersion.StartsWith("8.")) {
        Write-Host "Warning: This project requires .NET 8. Current version: $dotnetVersion" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error: .NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Green
dotnet restore

# Build the solution
Write-Host "Building the solution..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

# Create necessary directories
Write-Host "Creating necessary directories..." -ForegroundColor Green
New-Item -ItemType Directory -Force -Path "src\EduTrack.WebApp\App_Data" | Out-Null
New-Item -ItemType Directory -Force -Path "src\EduTrack.WebApp\wwwroot\storage" | Out-Null
New-Item -ItemType Directory -Force -Path "src\EduTrack.WebApp\App_Data\logs" | Out-Null

Write-Host "Development environment setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Yellow
Write-Host "  cd src\EduTrack.WebApp" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "The application will be available at: https://localhost:5001 or http://localhost:5000" -ForegroundColor Yellow
Write-Host ""
Write-Host "Default login credentials:" -ForegroundColor Yellow
Write-Host "  Admin: admin@local / Passw0rd!" -ForegroundColor White
Write-Host "  Teacher: teacher@local / Passw0rd!" -ForegroundColor White
Write-Host "  Student: student1@local / Passw0rd!" -ForegroundColor White
