using EduTrack.Application;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure;
using EduTrack.Infrastructure.Data;
using EduTrack.WebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("App_Data/logs/edutrack-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure database
var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString(databaseProvider);

builder.Services.AddDbContext<AppDbContext>(options =>
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

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Public/Account/Login";
    options.LogoutPath = "/Public/Account/Logout";
    options.AccessDeniedPath = "/Public/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Configure anti-forgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

var app = builder.Build();

// Ensure directories exist
var appDataPath = Path.Combine(app.Environment.ContentRootPath, "App_Data");
var storagePath = Path.Combine(app.Environment.WebRootPath, "storage");
var logsPath = Path.Combine(appDataPath, "logs");

Directory.CreateDirectory(appDataPath);
Directory.CreateDirectory(storagePath);
Directory.CreateDirectory(logsPath);

// Configure SQLite pragmas
if (databaseProvider.ToLowerInvariant() == "sqlite")
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.OpenConnection();
    context.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON; PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;");
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure area routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route redirects to Public area
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Public" });

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        await SeedData.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
    }
}

try
{
    Log.Information("Starting EduTrack WebApp");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }