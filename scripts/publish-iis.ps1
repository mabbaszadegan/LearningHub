# Windows IIS Publish Script
# This script publishes the application for IIS deployment

Write-Host "Publishing EduTrack for IIS deployment..." -ForegroundColor Green

# Clean and publish
dotnet clean
dotnet publish src/EduTrack.WebApp/EduTrack.WebApp.csproj -c Release -o ./publish/win --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed. Please check the errors above." -ForegroundColor Red
    exit 1
}

# Create necessary directories
New-Item -ItemType Directory -Force -Path "./publish/win/App_Data" | Out-Null
New-Item -ItemType Directory -Force -Path "./publish/win/wwwroot/storage" | Out-Null
New-Item -ItemType Directory -Force -Path "./publish/win/App_Data/logs" | Out-Null

# Copy configuration files
Copy-Item "src/EduTrack.WebApp/appsettings.json" "./publish/win/" -Force
Copy-Item "src/EduTrack.WebApp/appsettings.Production.json" "./publish/win/" -Force

Write-Host "Windows IIS publish completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To deploy on IIS:" -ForegroundColor Yellow
Write-Host "1. Copy the ./publish/win directory to your IIS server" -ForegroundColor White
Write-Host "2. Install .NET 8 Hosting Bundle on your server" -ForegroundColor White
Write-Host "3. Create a new IIS site pointing to the publish directory" -ForegroundColor White
Write-Host "4. Set the Application Pool to 'No Managed Code'" -ForegroundColor White
Write-Host "5. Grant write permissions to App_Data and wwwroot/storage folders" -ForegroundColor White
Write-Host ""
Write-Host "For detailed IIS setup instructions, see README.md" -ForegroundColor Yellow
