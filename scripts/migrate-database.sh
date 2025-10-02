#!/bin/bash

# Database Migration Script for Linux/Mac
PROVIDER="SqlServer"
CONNECTION_STRING=""
RESET=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--provider)
            PROVIDER="$2"
            shift 2
            ;;
        -c|--connection)
            CONNECTION_STRING="$2"
            shift 2
            ;;
        -r|--reset)
            RESET=true
            shift
            ;;
        *)
            echo "Unknown option $1"
            exit 1
            ;;
    esac
done

echo "=== EduTrack Database Migration ==="
echo "Provider: $PROVIDER"

# Set working directory to WebApp
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WEBAPP_PATH="$SCRIPT_DIR/../src/EduTrack.WebApp"
cd "$WEBAPP_PATH"

# Update appsettings.json with selected provider
if [ -f "appsettings.json" ]; then
    # Use jq to update JSON if available, otherwise use sed
    if command -v jq &> /dev/null; then
        jq --arg provider "$PROVIDER" '.Database.Provider = $provider' appsettings.json > tmp.json && mv tmp.json appsettings.json
        echo "Updated appsettings.json with provider: $PROVIDER"
    else
        echo "Warning: jq not found. Please manually update Database.Provider in appsettings.json to: $PROVIDER"
    fi
fi

# Update connection string if provided
if [ -n "$CONNECTION_STRING" ]; then
    if command -v jq &> /dev/null; then
        jq --arg provider "$PROVIDER" --arg conn "$CONNECTION_STRING" '.ConnectionStrings[$provider] = $conn' appsettings.json > tmp.json && mv tmp.json appsettings.json
        echo "Updated connection string for $PROVIDER"
    else
        echo "Warning: Please manually update ConnectionStrings.$PROVIDER in appsettings.json"
    fi
fi

# Run migrations
if [ "$RESET" = true ]; then
    echo "Resetting database..."
    dotnet ef database drop --force --project ../EduTrack.Infrastructure
fi

echo "Applying migrations..."
dotnet ef database update --project ../EduTrack.Infrastructure

if [ $? -eq 0 ]; then
    echo "Migration completed successfully!"
    
    # Run seeding
    echo "Running data seeding..."
    dotnet run --no-build -- --seed
    
    echo "Database setup completed!"
else
    echo "Migration failed!"
    exit 1
fi

echo "=== Migration Complete ==="
