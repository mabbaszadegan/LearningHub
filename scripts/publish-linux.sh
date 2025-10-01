#!/bin/bash

# Linux Publish Script
# This script publishes the application for Linux deployment

echo "Publishing EduTrack for Linux deployment..."

# Clean and publish
dotnet clean
dotnet publish src/EduTrack.WebApp/EduTrack.WebApp.csproj -c Release -o ./publish/linux --self-contained false

if [ $? -ne 0 ]; then
    echo "Publish failed. Please check the errors above."
    exit 1
fi

# Create necessary directories
mkdir -p ./publish/linux/App_Data
mkdir -p ./publish/linux/wwwroot/storage
mkdir -p ./publish/linux/App_Data/logs

# Set permissions
chmod +x ./publish/linux/EduTrack.WebApp
chmod 755 ./publish/linux/App_Data
chmod 755 ./publish/linux/wwwroot/storage
chmod 755 ./publish/linux/App_Data/logs

# Copy configuration files
cp src/EduTrack.WebApp/appsettings.json ./publish/linux/
cp src/EduTrack.WebApp/appsettings.Production.json ./publish/linux/

echo "Linux publish completed successfully!"
echo ""
echo "To deploy on Linux:"
echo "1. Copy the ./publish/linux directory to your server"
echo "2. Install .NET 8 Runtime on your server"
echo "3. Run: dotnet EduTrack.WebApp.dll"
echo ""
echo "For systemd service, see docs/edutrack.service"
