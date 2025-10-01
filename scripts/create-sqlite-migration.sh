#!/bin/bash

# SQLite Migration Script
# This script creates the initial migration for SQLite

echo "Creating SQLite migration..."

cd src/EduTrack.Infrastructure

# Create initial migration
dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Migrations/Sqlite

echo "SQLite migration created successfully!"
echo "To apply the migration, run: dotnet ef database update --context AppDbContext"
