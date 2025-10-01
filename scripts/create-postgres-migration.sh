#!/bin/bash

# PostgreSQL Migration Script
# This script creates the initial migration for PostgreSQL

echo "Creating PostgreSQL migration..."

cd src/EduTrack.Infrastructure

# Create initial migration for PostgreSQL
dotnet ef migrations add InitialCreate_Postgres --context AppDbContext --output-dir Migrations/Postgres

echo "PostgreSQL migration created successfully!"
echo "To apply the migration, run: dotnet ef database update --context AppDbContext"
