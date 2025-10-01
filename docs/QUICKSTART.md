# EduTrack - Learning Management System

## Project Overview

EduTrack is a production-grade, offline/intranet-first Learning Management System built with .NET 8 MVC. It provides comprehensive course management, assessment capabilities, progress tracking, and reporting features.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- SQLite (included), SQL Server, or PostgreSQL

### Quick Start

1. **Clone the repository**
2. **Run development setup:**
   ```bash
   # Windows
   .\scripts\dev-setup.ps1
   
   # Linux/macOS
   ./scripts/dev-setup.sh
   ```
3. **Start the application:**
   ```bash
   cd src/EduTrack.WebApp
   dotnet run
   ```
4. **Access at:** https://localhost:5001

### Default Login Credentials

- **Admin**: `admin@local` / `Passw0rd!`
- **Teacher**: `teacher@local` / `Passw0rd!`
- **Student**: `student1@local` / `Passw0rd!`

## Features

- ✅ Course Management (Create, Read, Update, Delete)
- ✅ Module and Lesson Management
- ✅ Resource Upload and Management
- ✅ Exam/Quiz System with Multiple Question Types
- ✅ Progress Tracking with Streak System
- ✅ Classroom Management
- ✅ PWA Support with Offline Capabilities
- ✅ Multi-Database Support (SQLite, SQL Server, PostgreSQL)
- ✅ Responsive Bootstrap 5 UI
- ✅ Authentication and Authorization
- ✅ CSV Export for Reports

## Architecture

- **Domain Layer**: Core business entities and logic
- **Application Layer**: CQRS with MediatR
- **Infrastructure Layer**: EF Core with multiple providers
- **WebApp Layer**: ASP.NET Core MVC with Areas

## Database Providers

### SQLite (Default)
```json
{
  "Database": {
    "Provider": "Sqlite",
    "ConnectionStrings": {
      "Sqlite": "Data Source=./App_Data/edutrack.db"
    }
  }
}
```

### SQL Server
```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStrings": {
      "SqlServer": "Server=localhost;Database=EduTrack;Trusted_Connection=True;TrustServerCertificate=True"
    }
  }
}
```

### PostgreSQL
```json
{
  "Database": {
    "Provider": "Postgres",
    "ConnectionStrings": {
      "Postgres": "Host=localhost;Database=edutrack;Username=postgres;Password=postgres"
    }
  }
}
```

## Deployment

### Windows IIS
```powershell
.\scripts\publish-iis.ps1
```

### Linux
```bash
./scripts/publish-linux.sh
```

## Testing

```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

MIT License - see LICENSE file for details.
