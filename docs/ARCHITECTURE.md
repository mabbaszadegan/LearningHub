# EduTrack Architecture Documentation

## Overview

EduTrack follows Clean Architecture principles with Domain-Driven Design (DDD) patterns. The solution is structured into distinct layers with clear separation of concerns and dependency inversion.

## Solution Structure

```
EduTrack/
├── src/
│   ├── EduTrack.Domain/           # Core business logic
│   ├── EduTrack.Application/      # Use cases and business rules
│   ├── EduTrack.Infrastructure/   # External concerns
│   ├── EduTrack.WebApp/          # Presentation layer
│   └── EduTrack.Tests/           # Test projects
├── migrations/                    # Database migrations
├── scripts/                      # Build and deployment scripts
├── docs/                         # Documentation
└── README.md
```

## Layer Descriptions

### Domain Layer (`EduTrack.Domain`)

The core of the application containing:

- **Entities**: Core business objects (User, Course, Module, Lesson, etc.)
- **Value Objects**: Immutable objects (Email)
- **Enums**: Domain-specific enumerations (QuestionType, ProgressStatus, etc.)
- **Domain Services**: Business logic that doesn't belong to entities

**Key Entities:**
- `User`: Extends IdentityUser with custom properties
- `Course`: Top-level course container
- `Module`: Course sections
- `Lesson`: Individual learning units
- `Resource`: Files and media associated with lessons
- `Question`: Assessment questions with choices
- `Exam`: Collections of questions with timing and scoring
- `Attempt`: Student exam attempts
- `Progress`: Student progress tracking

### Application Layer (`EduTrack.Application`)

Implements CQRS pattern with MediatR:

- **Commands**: Write operations (CreateCourse, UpdateCourse, etc.)
- **Queries**: Read operations (GetCourses, GetCourseById, etc.)
- **Handlers**: Business logic for commands and queries
- **DTOs**: Data transfer objects for API contracts
- **Interfaces**: Abstractions for external dependencies
- **Validation**: FluentValidation rules

**Key Patterns:**
- **CQRS**: Separate read and write models
- **MediatR**: Mediator pattern for decoupling
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management

### Infrastructure Layer (`EduTrack.Infrastructure`)

Handles external concerns:

- **Data Access**: Entity Framework Core with multiple providers
- **Repositories**: Concrete implementations of repository interfaces
- **Services**: File storage, logging, etc.
- **Configuration**: Database provider switching

**Database Providers:**
- SQLite (default, file-based)
- SQL Server (enterprise)
- PostgreSQL (open source)

### WebApp Layer (`EduTrack.WebApp`)

ASP.NET Core MVC presentation layer:

- **Controllers**: Handle HTTP requests
- **Views**: Razor pages with Bootstrap 5
- **Areas**: Admin, Teacher, Student sections
- **Authentication**: ASP.NET Core Identity
- **PWA**: Service worker and manifest

## Data Flow

1. **Request** → Controller receives HTTP request
2. **Command/Query** → Controller creates MediatR command/query
3. **Handler** → Application layer processes business logic
4. **Repository** → Infrastructure layer accesses data
5. **Response** → DTO returned through layers to controller
6. **View** → Controller renders view with data

## Database Design

### Key Relationships

- **User** → **Profile** (1:1)
- **Course** → **Module** (1:many)
- **Module** → **Lesson** (1:many)
- **Lesson** → **Resource** (1:many)
- **User** → **Class** (many:many via Enrollment)
- **Exam** → **Question** (many:many via ExamQuestion)
- **User** → **Attempt** (1:many)
- **User** → **Progress** (1:many)

### Concurrency Control

- SQL Server: `rowversion` columns
- SQLite: `byte[]` ConcurrencyToken
- PostgreSQL: `xmin` system column

### Indexing Strategy

- Primary keys (clustered)
- Foreign keys (non-clustered)
- Frequently queried columns (UserId, ClassId, etc.)
- Composite indexes for common query patterns

## Security Considerations

### Authentication & Authorization

- ASP.NET Core Identity for user management
- Cookie-based authentication
- Role-based authorization (Admin, Teacher, Student)
- Password policies enforced

### Data Protection

- Anti-forgery tokens on forms
- SQL injection prevention via EF Core
- XSS protection via Razor encoding
- HTTPS enforcement in production

### File Upload Security

- File type validation
- Size limits
- Secure file storage in `wwwroot/storage`
- Path traversal prevention

## Performance Optimizations

### Database

- Connection pooling
- Query optimization with EF Core
- Indexed columns for common lookups
- Pagination for large datasets

### Caching

- Static file caching
- Response caching for read-only data
- PWA service worker caching

### PWA Features

- Offline-first design
- Background sync for submissions
- Resource preloading
- App shell caching

## Testing Strategy

### Unit Tests

- Domain entities and value objects
- Application command/query handlers
- Business logic validation

### Integration Tests

- Controller actions
- Database operations
- End-to-end workflows

### Test Infrastructure

- In-memory database for testing
- Mock services for external dependencies
- Test data builders
- WebApplicationFactory for integration tests

## Deployment Architecture

### Development

- SQLite file database
- IIS Express or Kestrel
- Hot reload enabled
- Detailed logging

### Production

- SQL Server or PostgreSQL
- IIS (Windows) or Kestrel + systemd (Linux)
- Nginx reverse proxy (optional)
- Structured logging with Serilog

### Scaling Considerations

- Stateless application design
- Database connection pooling
- CDN for static assets
- Load balancing ready

## Configuration Management

### Environment-Specific Settings

- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- `appsettings.Production.json`: Production settings

### Database Provider Switching

- Runtime provider selection via configuration
- Provider-specific migrations
- Connection string management

### Feature Flags

- Offline mode toggle
- PWA features
- Logging levels

## Monitoring & Logging

### Structured Logging

- Serilog with console and file sinks
- Correlation IDs for request tracking
- Activity logging for audit trails

### Health Checks

- Database connectivity
- File system access
- External service availability

### Performance Monitoring

- Request/response timing
- Database query performance
- Memory usage tracking

## Future Enhancements

### Planned Features

- Real-time notifications
- Advanced reporting and analytics
- Mobile app (React Native/Xamarin)
- Integration with external LMS systems
- Multi-tenancy support

### Technical Improvements

- Microservices architecture
- Event sourcing for audit trails
- Advanced caching strategies
- API versioning
- GraphQL support

## Development Guidelines

### Code Standards

- C# 12 features
- Nullable reference types enabled
- Analyzers enabled
- Consistent naming conventions

### Git Workflow

- Feature branches
- Pull request reviews
- Automated testing
- Semantic versioning

### Documentation

- XML documentation for public APIs
- Architecture decision records (ADRs)
- API documentation
- User guides

---

This architecture provides a solid foundation for a scalable, maintainable LMS while supporting offline-first requirements and multiple deployment scenarios.
