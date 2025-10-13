using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Repositories;
using EduTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString(databaseProvider);

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (databaseProvider.ToLowerInvariant())
            {
                case "sqlite":
                    options.UseSqlite(connectionString, sqliteOptions =>
                    {
                        sqliteOptions.CommandTimeout(30);
                    });
                    break;
                case "sqlserver":
                    options.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.CommandTimeout(30);
                        sqlServerOptions.EnableRetryOnFailure(3);
                    });
                    break;
                case "postgres":
                    options.UseNpgsql(connectionString, postgresOptions =>
                    {
                        postgresOptions.CommandTimeout(30);
                        postgresOptions.EnableRetryOnFailure(3);
                    });
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
            }
        });

        // Register generic repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register specific repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();
        services.AddScoped<IEducationalContentRepository, EducationalContentRepository>();
        services.AddScoped<ITeachingPlanRepository, TeachingPlanRepository>();
        services.AddScoped<IStudentGroupRepository, StudentGroupRepository>();
        services.AddScoped<IScheduleItemRepository, ScheduleItemRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<ITeachingSessionReportRepository, TeachingSessionReportRepository>();
        services.AddScoped<ITeachingSessionAttendanceRepository, TeachingSessionAttendanceRepository>();
        services.AddScoped<ITeachingSessionExecutionRepository, TeachingSessionExecutionRepository>();
        services.AddScoped<ITeachingSessionTopicCoverageRepository, TeachingSessionTopicCoverageRepository>();
        services.AddScoped<ITeachingPlanProgressRepository, TeachingPlanProgressRepository>();
        services.AddScoped<ISubChapterRepository, SubChapterRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IFileRepository, FileRepository>();

        // Register infrastructure services
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }
}
