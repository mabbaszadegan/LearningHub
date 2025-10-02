using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.WebApp.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create roles
        await CreateRolesAsync(roleManager);

        // Create admin user
        await CreateAdminUserAsync(userManager);

        // Create sample data if database is empty
        if (!await context.Courses.AnyAsync())
        {
            await CreateSampleDataAsync(context, userManager);
        }
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task CreateAdminUserAsync(UserManager<User> userManager)
    {
        var adminEmail = "admin@local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Passw0rd!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task CreateSampleDataAsync(AppDbContext context, UserManager<User> userManager)
    {
        var now = DateTimeOffset.UtcNow;

        // Create teachers
        var teacherEmail = "teacher@local";
        var teacher = await userManager.FindByEmailAsync(teacherEmail);
        if (teacher == null)
        {
            teacher = new User
            {
                UserName = teacherEmail,
                Email = teacherEmail,
                FirstName = "احمد",
                LastName = "احمدی",
                Role = UserRole.Teacher,
                EmailConfirmed = true,
                CreatedAt = now,
                IsActive = true
            };
            await userManager.CreateAsync(teacher, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher, "Teacher");
        }

        // Create additional teachers
        var teacher2Email = "teacher2@local";
        var teacher2 = await userManager.FindByEmailAsync(teacher2Email);
        if (teacher2 == null)
        {
            teacher2 = new User
            {
                UserName = teacher2Email,
                Email = teacher2Email,
                FirstName = "فاطمه",
                LastName = "محمدی",
                Role = UserRole.Teacher,
                EmailConfirmed = true,
                CreatedAt = now,
                IsActive = true
            };
            await userManager.CreateAsync(teacher2, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher2, "Teacher");
        }

        var teacher3Email = "teacher3@local";
        var teacher3 = await userManager.FindByEmailAsync(teacher3Email);
        if (teacher3 == null)
        {
            teacher3 = new User
            {
                UserName = teacher3Email,
                Email = teacher3Email,
                FirstName = "علی",
                LastName = "رضایی",
                Role = UserRole.Teacher,
                EmailConfirmed = true,
                CreatedAt = now,
                IsActive = true
            };
            await userManager.CreateAsync(teacher3, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher3, "Teacher");
        }

        // Create students
        var student1Email = "student1@local";
        var student1 = await userManager.FindByEmailAsync(student1Email);
        if (student1 == null)
        {
            student1 = new User
            {
                UserName = student1Email,
                Email = student1Email,
                FirstName = "Alice",
                LastName = "Student",
                Role = UserRole.Student,
                EmailConfirmed = true,
                CreatedAt = now,
                IsActive = true
            };
            await userManager.CreateAsync(student1, "Passw0rd!");
            await userManager.AddToRoleAsync(student1, "Student");
        }

        var student2Email = "student2@local";
        var student2 = await userManager.FindByEmailAsync(student2Email);
        if (student2 == null)
        {
            student2 = new User
            {
                UserName = student2Email,
                Email = student2Email,
                FirstName = "Bob",
                LastName = "Student",
                Role = UserRole.Student,
                EmailConfirmed = true,
                CreatedAt = now,
                IsActive = true
            };
            await userManager.CreateAsync(student2, "Passw0rd!");
            await userManager.AddToRoleAsync(student2, "Student");
        }

        // Create course
        var course = new Course
        {
            Title = "General English A1",
            Description = "Beginner level English course covering basic grammar, vocabulary, and conversation skills.",
            IsActive = true,
            Order = 1,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = teacher.Id
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Create module
        var module = new Module
        {
            CourseId = course.Id,
            Title = "Introduction to English",
            Description = "Basic introduction to English language fundamentals.",
            IsActive = true,
            Order = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Modules.Add(module);
        await context.SaveChangesAsync();

        // Create lessons
        var lesson1 = new Lesson
        {
            ModuleId = module.Id,
            Title = "Greetings and Introductions",
            Content = "Learn how to greet people and introduce yourself in English.",
            IsActive = true,
            Order = 1,
            DurationMinutes = 30,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Lessons.Add(lesson1);

        var lesson2 = new Lesson
        {
            ModuleId = module.Id,
            Title = "Numbers and Colors",
            Content = "Learn basic numbers and colors in English.",
            IsActive = true,
            Order = 2,
            DurationMinutes = 25,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Lessons.Add(lesson2);
        await context.SaveChangesAsync();

        // Create resources
        var resource1 = new Resource
        {
            LessonId = lesson1.Id,
            Title = "Greetings Vocabulary",
            Description = "PDF with common greeting phrases",
            Type = ResourceType.PDF,
            FilePath = "greetings.pdf",
            IsActive = true,
            Order = 1,
            CreatedAt = now
        };
        context.Resources.Add(resource1);

        var resource2 = new Resource
        {
            LessonId = lesson2.Id,
            Title = "Numbers Video",
            Description = "Video lesson on English numbers",
            Type = ResourceType.Video,
            Url = "https://example.com/numbers-video",
            IsActive = true,
            Order = 1,
            CreatedAt = now
        };
        context.Resources.Add(resource2);
        await context.SaveChangesAsync();

        // Create class
        var classEntity = new Class
        {
            CourseId = course.Id,
            Name = "English A1 - Class 1",
            Description = "First class for English A1 course",
            TeacherId = teacher.Id,
            StartDate = now.AddDays(7),
            EndDate = now.AddDays(90),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Classes.Add(classEntity);
        await context.SaveChangesAsync();

        // Create enrollments
        var enrollment1 = new Enrollment
        {
            ClassId = classEntity.Id,
            StudentId = student1.Id,
            EnrolledAt = now,
            IsActive = true
        };
        context.Enrollments.Add(enrollment1);

        var enrollment2 = new Enrollment
        {
            ClassId = classEntity.Id,
            StudentId = student2.Id,
            EnrolledAt = now,
            IsActive = true
        };
        context.Enrollments.Add(enrollment2);
        await context.SaveChangesAsync();

        // Create questions
        var questions = new List<Question>
        {
            new Question
            {
                Text = "What is the correct way to greet someone in English?",
                Type = QuestionType.MultipleChoice,
                Explanation = "Hello is the most common greeting in English.",
                Points = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = teacher.Id
            },
            new Question
            {
                Text = "The word 'red' is a color.",
                Type = QuestionType.TrueFalse,
                Explanation = "Red is indeed a color.",
                Points = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = teacher.Id
            },
            new Question
            {
                Text = "How do you say 'thank you' in English?",
                Type = QuestionType.ShortAnswer,
                Explanation = "Thank you is the correct way to express gratitude.",
                Points = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = teacher.Id
            }
        };
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();

        // Create choices for multiple choice question
        var choice1 = new Choice
        {
            QuestionId = questions[0].Id,
            Text = "Hello",
            IsCorrect = true,
            Order = 1
        };
        var choice2 = new Choice
        {
            QuestionId = questions[0].Id,
            Text = "Goodbye",
            IsCorrect = false,
            Order = 2
        };
        var choice3 = new Choice
        {
            QuestionId = questions[0].Id,
            Text = "See you later",
            IsCorrect = false,
            Order = 3
        };
        context.Choices.AddRange(choice1, choice2, choice3);
        await context.SaveChangesAsync();

        // Create exam
        var exam = new Exam
        {
            Title = "English A1 Assessment",
            Description = "Basic assessment for English A1 level",
            DurationMinutes = 30,
            PassingScore = 75,
            ShowSolutions = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = teacher.Id
        };
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        // Add questions to exam
        var examQuestions = new List<ExamQuestion>
        {
            new ExamQuestion { ExamId = exam.Id, QuestionId = questions[0].Id, Order = 1 },
            new ExamQuestion { ExamId = exam.Id, QuestionId = questions[1].Id, Order = 2 },
            new ExamQuestion { ExamId = exam.Id, QuestionId = questions[2].Id, Order = 3 }
        };
        context.ExamQuestions.AddRange(examQuestions);
        await context.SaveChangesAsync();
    }
}
