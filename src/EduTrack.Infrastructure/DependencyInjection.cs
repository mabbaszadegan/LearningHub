using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
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

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
