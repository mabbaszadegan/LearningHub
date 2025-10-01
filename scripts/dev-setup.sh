#!/bin/bash

# Development Setup Script for Linux/macOS
# This script sets up the development environment for EduTrack

echo "Setting up EduTrack development environment..."

# Check if .NET 8 is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET 8 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "Found .NET version: $DOTNET_VERSION"

if [[ ! "$DOTNET_VERSION" =~ ^8\. ]]; then
    echo "Warning: This project requires .NET 8. Current version: $DOTNET_VERSION"
fi

# Restore packages
echo "Restoring NuGet packages..."
dotnet restore

# Build the solution
echo "Building the solution..."
dotnet build

if [ $? -ne 0 ]; then
    echo "Build failed. Please check the errors above."
    exit 1
fi

# Create necessary directories
echo "Creating necessary directories..."
mkdir -p src/EduTrack.WebApp/App_Data
mkdir -p src/EduTrack.WebApp/wwwroot/storage
mkdir -p src/EduTrack.WebApp/App_Data/logs

# Set permissions for directories
chmod 755 src/EduTrack.WebApp/App_Data
chmod 755 src/EduTrack.WebApp/wwwroot/storage
chmod 755 src/EduTrack.WebApp/App_Data/logs

echo "Development environment setup complete!"
echo ""
echo "To run the application:"
echo "  cd src/EduTrack.WebApp"
echo "  dotnet run"
echo ""
echo "The application will be available at: https://localhost:5001 or http://localhost:5000"
echo ""
echo "Default login credentials:"
echo "  Admin: admin@local / Passw0rd!"
echo "  Teacher: teacher@local / Passw0rd!"
echo "  Student: student1@local / Passw0rd!"
