#!/bin/bash

# SQL Server Migration Script
# This script creates the initial migration for SQL Server

echo "Creating SQL Server migration..."

cd src/EduTrack.Infrastructure

# Create initial migration for SQL Server
dotnet ef migrations add InitialCreate_SqlServer --context AppDbContext --output-dir Migrations/SqlServer

echo "SQL Server migration created successfully!"
echo "To apply the migration, run: dotnet ef database update --context AppDbContext"
