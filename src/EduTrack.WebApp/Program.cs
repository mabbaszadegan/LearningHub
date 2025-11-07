using EduTrack.Application;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure;
using EduTrack.Infrastructure.Data;
using EduTrack.WebApp.Data;
using EduTrack.WebApp.Filters;
using EduTrack.WebApp.ModelBinders;
using EduTrack.WebApp.Services;
using Microsoft.AspNetCore.Authentication;
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
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<DomainExceptionFilter>();
    options.ModelBinderProviders.Insert(0, new PersianDateTimeOffsetModelBinderProvider());
})
    .AddRazorRuntimeCompilation();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Simplified password requirements
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false; // Allow duplicate emails for students
    
    // Email confirmation not required
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
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

// Add claims transformer to include user roles in claims
builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();
builder.Services.AddScoped<IStudentProfileContext, StudentProfileContext>();

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
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
var uploadsTmpPath = Path.Combine(uploadsPath, "tmp");

Directory.CreateDirectory(appDataPath);
Directory.CreateDirectory(storagePath);
Directory.CreateDirectory(logsPath);
Directory.CreateDirectory(uploadsPath);
Directory.CreateDirectory(uploadsTmpPath);

// Configure SQLite pragmas
var databaseProvider = builder.Configuration.GetValue<string>("Database:Provider", "SQLite");
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

// Configure static file serving for uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true, // Allow serving audio files
    OnPrepareResponse = ctx =>
    {
        // Set appropriate headers for audio files
        if (ctx.File.Name.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "audio/mpeg");
            ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");
        }
    }
});

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Configure area routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Configure non-area controllers (main controllers)
app.MapControllerRoute(
    name: "main",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Default route redirects to Public area
app.MapControllerRoute(
    name: "default",
    pattern: "",
    defaults: new { area = "Public", controller = "Home", action = "Index" });

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Apply pending migrations automatically if enabled
        var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", true);
        if (autoMigrate)
        {
            Log.Information("Applying database migrations...");
            await context.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
        else
        {
            Log.Information("Auto-migration is disabled. Please apply migrations manually.");
        }
        
        await SeedData.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying migrations or seeding the database");
        throw; // Re-throw to prevent app from starting with inconsistent database
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