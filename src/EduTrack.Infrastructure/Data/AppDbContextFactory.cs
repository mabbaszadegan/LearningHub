using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EduTrack.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EduTrack.WebApp"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get database provider and connection string
        var databaseProvider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString(databaseProvider);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string for '{databaseProvider}' not found.");
        }

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        switch (databaseProvider.ToLowerInvariant())
        {
            case "sqlite":
                optionsBuilder.UseSqlite(connectionString);
                break;
            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case "postgres":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            default:
                throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
        }

        return new AppDbContext(optionsBuilder.Options, databaseProvider);
    }
}
