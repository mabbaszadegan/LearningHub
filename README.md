# EduTrack - Offline Learning Management System

EduTrack is a production-grade, offline/intranet-first Learning Management System (LMS) built with .NET 8 MVC. It provides comprehensive course management, exam/quiz functionality, progress tracking, and reporting capabilities designed for offline or intranet environments.

## 🚀 Features

- **Course Management**: Create and manage courses, modules, lessons, and resources
- **Assessment System**: Build exams with multiple choice, true/false, and short answer questions
- **Progress Tracking**: Monitor student progress with streak tracking and mastery levels
- **Classroom Management**: Organize students into classes with enrollment tracking
- **Offline Capabilities**: PWA support with offline lesson viewing and queued submissions
- **Multi-Database Support**: SQLite (default), SQL Server, and PostgreSQL
- **Clean Architecture**: Domain-driven design with CQRS pattern
- **Responsive UI**: Bootstrap 5 with mobile-friendly design

## 🏗️ Architecture

EduTrack follows Clean Architecture principles with the following layers:

- **Domain**: Core business entities, value objects, and enums
- **Application**: CQRS with MediatR, DTOs, and validation
- **Infrastructure**: EF Core, repositories, and external services
- **WebApp**: ASP.NET Core MVC with Areas (Admin, Teacher, Student)
- **Tests**: Unit and integration tests

## 📋 Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code (optional)
- SQLite (included), SQL Server, or PostgreSQL (for production)

## 🛠️ Quick Start

### Development Setup

**Windows:**
```powershell
.\scripts\dev-setup.ps1
```

**Linux/macOS:**
```bash
chmod +x scripts/dev-setup.sh
./scripts/dev-setup.sh
```

### Manual Setup

1. **Clone and restore packages:**
   ```bash
   git clone <repository-url>
   cd EduTrack
   dotnet restore
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run the application:**
   ```bash
   cd src/EduTrack.WebApp
   dotnet run
   ```

4. **Access the application:**
   - HTTPS: https://localhost:5001
   - HTTP: http://localhost:5000

### Default Login Credentials

- **Admin**: `admin@local` / `Passw0rd!`
- **Teacher**: `teacher@local` / `Passw0rd!`
- **Student**: `student1@local` / `Passw0rd!`

## 🗄️ Database Configuration

EduTrack supports three database providers:

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

### Switching Database Providers

1. **Update appsettings.json** with the desired provider and connection string
2. **Create migration** for the new provider:
   ```bash
   # SQL Server
   .\scripts\create-sqlserver-migration.ps1
   
   # PostgreSQL
   .\scripts\create-postgres-migration.ps1
   ```
3. **Apply migration**:
   ```bash
   dotnet ef database update --context AppDbContext
   ```

## 📦 Deployment

### Windows IIS Deployment

1. **Publish the application:**
   ```powershell
   .\scripts\publish-iis.ps1
   ```

2. **IIS Setup:**
   - Install .NET 8 Hosting Bundle
   - Create new IIS site pointing to `./publish/win`
   - Set Application Pool to "No Managed Code"
   - Grant write permissions to `App_Data` and `wwwroot/storage` folders

### Linux Deployment

1. **Publish the application:**
   ```bash
   chmod +x scripts/publish-linux.sh
   ./scripts/publish-linux.sh
   ```

2. **Install .NET 8 Runtime:**
   ```bash
   # Ubuntu/Debian
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y dotnet-runtime-8.0
   ```

3. **Setup systemd service:**
   ```bash
   sudo cp docs/edutrack.service /etc/systemd/system/
   sudo systemctl daemon-reload
   sudo systemctl enable edutrack
   sudo systemctl start edutrack
   ```

4. **Optional: Nginx reverse proxy** (see `docs/nginx.conf`)

## 🧪 Testing

Run the test suite:

```bash
dotnet test
```

### Test Categories

- **Unit Tests**: Domain and Application layer tests
- **Integration Tests**: Controller and API tests
- **Database Tests**: Provider-specific migration tests

## 📱 PWA Features

EduTrack includes Progressive Web App capabilities:

- **Offline Support**: Cache static resources and lesson content
- **Installable**: Add to home screen on mobile devices
- **Background Sync**: Queue exam submissions when offline
- **Push Notifications**: (Future enhancement)

### PWA Configuration

The PWA is configured via `wwwroot/manifest.webmanifest` and `wwwroot/js/sw.js`.

## 🔧 Configuration

### File Storage

Resources are stored in `wwwroot/storage` by default. Configure the path in `appsettings.json`:

```json
{
  "Paths": {
    "StorageRoot": "wwwroot/storage"
  }
}
```

### Logging

Serilog is configured for console and file logging:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "App_Data/logs/edutrack-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 📊 Progress Tracking

EduTrack implements a sophisticated progress tracking system:

- **Streak Tracking**: Consecutive correct answers
- **Mastery Levels**: NotStarted → InProgress → Done → Mastered
- **Pass Threshold**: 75% required to pass exams
- **Streak Rules**: 
  - Pass (≥75%): `correct_count += 1`, `streak += 1`
  - Fail (<75%): `correct_count = max(0, cc-3)`, `streak = 0`
  - Status progression: 10 consecutive passes → Done, 20 → Mastered

## 🎯 Usage Examples

### Creating a Course

1. Navigate to **Catalog** → **Create Course**
2. Fill in course details (title, description, thumbnail)
3. Add modules and lessons
4. Upload resources (PDFs, videos, images)

### Building an Exam

1. Go to **Exams** → **Create Exam**
2. Set duration, passing score, and other options
3. Add questions from the question bank
4. Configure question order and points

### Managing Classes

1. Navigate to **Classroom** → **Create Class**
2. Select a course and set dates
3. Enroll students
4. Monitor progress and generate reports

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For issues and questions:

1. Check the [Issues](../../issues) page
2. Review the documentation
3. Create a new issue with detailed information

## 🔄 Version History

- **v1.0.0**: Initial release with core LMS functionality
- **v1.1.0**: Added PWA support and offline capabilities
- **v1.2.0**: Enhanced progress tracking and reporting

---

**EduTrack** - Empowering education through technology, offline-first.
