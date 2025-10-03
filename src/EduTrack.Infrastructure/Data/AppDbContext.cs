using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>
{
    private readonly IDatabaseProviderConfiguration _providerConfig;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // Initialize provider config with a safe default
        _providerConfig = new SqlServerConfiguration();
        
        // Try to get provider name from Database and update config if possible
        try
        {
            var providerName = Database?.ProviderName;
            if (!string.IsNullOrEmpty(providerName))
            {
                _providerConfig = DatabaseProviderFactory.GetConfiguration(providerName);
            }
        }
        catch
        {
            // Keep the default SqlServer configuration
        }
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, string databaseProvider) : base(options)
    {
        _providerConfig = DatabaseProviderFactory.GetConfiguration(databaseProvider switch
        {
            "Sqlite" => "Microsoft.EntityFrameworkCore.Sqlite",
            "SqlServer" => "Microsoft.EntityFrameworkCore.SqlServer", 
            "Postgres" => "Npgsql.EntityFrameworkCore.PostgreSQL",
            _ => "Microsoft.EntityFrameworkCore.SqlServer"
        });
    }

    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamQuestion> ExamQuestions { get; set; }
    public DbSet<Attempt> Attempts { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Progress> Progresses { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<SubChapter> SubChapters { get; set; }
    public DbSet<Domain.Entities.File> Files { get; set; }
    public DbSet<EducationalContent> EducationalContents { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Course entity

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Configure Profile entity
        builder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasOne(e => e.User)
                .WithOne(e => e.Profile)
                .HasForeignKey<Profile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Course entity
        builder.Entity<Course>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Thumbnail).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.HasIndex(e => e.Order);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Module entity
        builder.Entity<Module>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasOne(e => e.Course)
                .WithMany(e => e.Modules)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CourseId, e.Order });
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Lesson entity
        builder.Entity<Lesson>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            (_providerConfig ?? new SqlServerConfiguration()).ConfigureLongText<Lesson>(entity.Property(e => e.Content));
            entity.Property(e => e.VideoUrl).HasMaxLength(500);
            entity.HasOne(e => e.Module)
                .WithMany(e => e.Lessons)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ModuleId, e.Order });
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Resource entity
        builder.Entity<Resource>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.HasOne(e => e.Lesson)
                .WithMany(e => e.Resources)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.LessonId, e.Order });
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Class entity
        builder.Entity<Class>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TeacherId).HasMaxLength(450).IsRequired();
            entity.HasOne(e => e.Course)
                .WithMany(e => e.Classes)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Teacher)
                .WithMany(e => e.Classes)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.TeacherId);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Enrollment entity
        builder.Entity<Enrollment>(entity =>
        {
            entity.Property(e => e.StudentId).HasMaxLength(450).IsRequired();
            entity.HasOne(e => e.Class)
                .WithMany(e => e.Enrollments)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany(e => e.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.ClassId, e.StudentId }).IsUnique();
        });

        // Configure Question entity
        builder.Entity<Question>(entity =>
        {
            entity.Property(e => e.Text).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Explanation).HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Type);
        });

        // Configure Choice entity
        builder.Entity<Choice>(entity =>
        {
            entity.Property(e => e.Text).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.Question)
                .WithMany(e => e.Choices)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.QuestionId, e.Order });
        });

        // Configure Exam entity
        builder.Entity<Exam>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure ExamQuestion entity
        builder.Entity<ExamQuestion>(entity =>
        {
            entity.HasOne(e => e.Exam)
                .WithMany(e => e.ExamQuestions)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                .WithMany(e => e.ExamQuestions)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ExamId, e.Order });
            entity.HasIndex(e => new { e.ExamId, e.QuestionId }).IsUnique();
        });

        // Configure Attempt entity
        builder.Entity<Attempt>(entity =>
        {
            entity.Property(e => e.StudentId).HasMaxLength(450).IsRequired();
            entity.HasOne(e => e.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany(e => e.Attempts)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => e.SubmittedAt);
        });

        // Configure Answer entity
        builder.Entity<Answer>(entity =>
        {
            entity.Property(e => e.TextAnswer).HasMaxLength(2000);
            entity.HasOne(e => e.Attempt)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                .WithMany(e => e.Answers)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SelectedChoice)
                .WithMany()
                .HasForeignKey(e => e.SelectedChoiceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.AttemptId);
            entity.HasIndex(e => e.QuestionId);
        });

        // Configure Progress entity
        builder.Entity<Progress>(entity =>
        {
            entity.Property(e => e.StudentId).HasMaxLength(450).IsRequired();
            entity.HasOne(e => e.Student)
                .WithMany(e => e.Progresses)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Lesson)
                .WithMany(e => e.Progresses)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Exam)
                .WithMany()
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => e.Status);
        });

        // Configure Chapter entity
        builder.Entity<Chapter>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Objective).HasMaxLength(2000).IsRequired();
            entity.HasOne(e => e.Course)
                .WithMany(e => e.Chapters)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CourseId, e.Order });
            entity.HasIndex(e => e.IsActive);
        });

        // Configure SubChapter entity
        builder.Entity<SubChapter>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Objective).HasMaxLength(2000).IsRequired();
            entity.HasOne(e => e.Chapter)
                .WithMany(e => e.SubChapters)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ChapterId, e.Order });
            entity.HasIndex(e => e.IsActive);
        });

        // Configure File entity
        builder.Entity<Domain.Entities.File>(entity =>
        {
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.MD5Hash).HasMaxLength(32).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.HasIndex(e => e.MD5Hash).IsUnique();
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure EducationalContent entity
        builder.Entity<EducationalContent>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ExternalUrl).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            (_providerConfig ?? new SqlServerConfiguration()).ConfigureLongText<EducationalContent>(entity.Property(e => e.TextContent));
            entity.HasOne(e => e.SubChapter)
                .WithMany(e => e.EducationalContents)
                .HasForeignKey(e => e.SubChapterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.File)
                .WithMany(e => e.EducationalContents)
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.SubChapterId, e.Order });
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.FileId);
        });

        // Configure ActivityLog entity
        builder.Entity<ActivityLog>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(e => e.ActivityLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Action);
        });

    }
}
