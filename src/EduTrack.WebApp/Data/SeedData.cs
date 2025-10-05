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
            adminUser = User.Create("Admin", "User", adminEmail);
            adminUser.EmailConfirmed = true;

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
            teacher = User.Create("احمد", "احمدی", teacherEmail);
            teacher.EmailConfirmed = true;
            await userManager.CreateAsync(teacher, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher, "Teacher");
        }

        // Create additional teachers
        var teacher2Email = "teacher2@local";
        var teacher2 = await userManager.FindByEmailAsync(teacher2Email);
        if (teacher2 == null)
        {
            teacher2 = User.Create("فاطمه", "محمدی", teacher2Email);
            teacher2.EmailConfirmed = true;
            await userManager.CreateAsync(teacher2, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher2, "Teacher");
        }

        var teacher3Email = "teacher3@local";
        var teacher3 = await userManager.FindByEmailAsync(teacher3Email);
        if (teacher3 == null)
        {
            teacher3 = User.Create("علی", "رضایی", teacher3Email);
            teacher3.EmailConfirmed = true;
            await userManager.CreateAsync(teacher3, "Passw0rd!");
            await userManager.AddToRoleAsync(teacher3, "Teacher");
        }

        // Create students
        var student1Email = "student1@local";
        var student1 = await userManager.FindByEmailAsync(student1Email);
        if (student1 == null)
        {
            student1 = User.Create("Alice", "Student", student1Email);
            student1.EmailConfirmed = true;
            await userManager.CreateAsync(student1, "Passw0rd!");
            await userManager.AddToRoleAsync(student1, "Student");
        }

        var student2Email = "student2@local";
        var student2 = await userManager.FindByEmailAsync(student2Email);
        if (student2 == null)
        {
            student2 = User.Create("Bob", "Student", student2Email);
            student2.EmailConfirmed = true;
            await userManager.CreateAsync(student2, "Passw0rd!");
            await userManager.AddToRoleAsync(student2, "Student");
        }

        // Create course
        var course = Course.Create(
            "General English A1",
            "Beginner level English course covering basic grammar, vocabulary, and conversation skills.",
            null,
            1,
            teacher.Id);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Create module
        var module = Module.Create(
            course.Id,
            "Introduction to English",
            "Basic introduction to English language fundamentals.",
            1);
        context.Modules.Add(module);
        await context.SaveChangesAsync();

        // Create lessons
        var lesson1 = Lesson.Create(
            module.Id,
            "Greetings and Introductions",
            "Learn how to greet people and introduce yourself in English.",
            null,
            30,
            1);
        context.Lessons.Add(lesson1);

        var lesson2 = Lesson.Create(
            module.Id,
            "Numbers and Colors",
            "Learn basic numbers and colors in English.",
            null,
            25,
            2);
        context.Lessons.Add(lesson2);
        await context.SaveChangesAsync();

        // Create resources
        var resource1 = Resource.Create(
            lesson1.Id,
            "Greetings Vocabulary",
            ResourceType.PDF,
            "greetings.pdf",
            null,
            null,
            null,
            1);
        context.Resources.Add(resource1);

        var resource2 = Resource.Create(
            lesson2.Id,
            "Numbers Video",
            ResourceType.Video,
            "-",
            "https://example.com/numbers-video",
            null,
            null,
            1);
        context.Resources.Add(resource2);
        await context.SaveChangesAsync();

        // Create class
        var classEntity = Class.Create(
            course.Id,
            "English A1 - Class 1",
            "First class for English A1 course",
            teacher.Id,
            now.AddDays(7),
            now.AddDays(90));
        context.Classes.Add(classEntity);
        await context.SaveChangesAsync();

        // Create enrollments
        var enrollment1 = Enrollment.Create(classEntity.Id, student1.Id);
        context.Enrollments.Add(enrollment1);

        var enrollment2 = Enrollment.Create(classEntity.Id, student2.Id);
        context.Enrollments.Add(enrollment2);
        await context.SaveChangesAsync();

        // Create questions
        var questions = new List<Question>
        {
            Question.Create(
                "What is the correct way to greet someone in English?",
                QuestionType.MultipleChoice,
                "Hello is the most common greeting in English.",
                1,
                teacher.Id),
            Question.Create(
                "The word 'red' is a color.",
                QuestionType.TrueFalse,
                "Red is indeed a color.",
                1,
                teacher.Id),
            Question.Create(
                "How do you say 'thank you' in English?",
                QuestionType.ShortAnswer,
                "Thank you is the correct way to express gratitude.",
                1,
                teacher.Id)
        };
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();

        // Create choices for multiple choice question
        var choice1 = Choice.Create(questions[0].Id, "Hello", true, 1);
        var choice2 = Choice.Create(questions[0].Id, "Goodbye", false, 2);
        var choice3 = Choice.Create(questions[0].Id, "See you later", false, 3);
        context.Choices.AddRange(choice1, choice2, choice3);
        await context.SaveChangesAsync();

        // Create exam
        var exam = Exam.Create(
            "English A1 Assessment",
            "Basic assessment for English A1 level",
            30,
            75,
            true,
            teacher.Id);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        // Add questions to exam
        var examQuestions = new List<ExamQuestion>
        {
            ExamQuestion.Create(exam.Id, questions[0].Id, 1),
            ExamQuestion.Create(exam.Id, questions[1].Id, 2),
            ExamQuestion.Create(exam.Id, questions[2].Id, 3)
        };
        context.ExamQuestions.AddRange(examQuestions);
        await context.SaveChangesAsync();
    }
}
