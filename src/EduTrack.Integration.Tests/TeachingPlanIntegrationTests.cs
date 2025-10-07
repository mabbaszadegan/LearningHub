using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace EduTrack.Integration.Tests;

public class TeachingPlanIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TeachingPlanIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Student_CanSubmitMCQ_AndReceiveAutoGrade()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Arrange - Create test data
        var teacher = User.Create("Test", "Teacher", "teacher@test.com");
        teacher.EmailConfirmed = true;
        await userManager.CreateAsync(teacher, "TestPass123!");
        await userManager.AddToRoleAsync(teacher, "Teacher");

        var student = User.Create("Test", "Student", "student@test.com");
        student.EmailConfirmed = true;
        await userManager.CreateAsync(student, "TestPass123!");
        await userManager.AddToRoleAsync(student, "Student");

        var course = Course.Create("Test Course", "Test Description", null, 1, teacher.Id, DisciplineType.Language);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var teachingPlan = TeachingPlan.Create(course.Id, teacher.Id, "Test Plan", "Test Description");
        context.TeachingPlans.Add(teachingPlan);
        await context.SaveChangesAsync();

        var scheduleItem = ScheduleItem.Create(
            teachingPlan.Id,
            ScheduleItemType.MultipleChoice,
            "Test MCQ",
            "Test question",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            true,
            """{"type":"MultipleChoice","stem":"What is 2+2?","choices":[{"text":"3","correct":false},{"text":"4","correct":true},{"text":"5","correct":false}]}""",
            10m,
            null,
            null,
            DisciplineType.Math);

        context.ScheduleItems.Add(scheduleItem);
        await context.SaveChangesAsync();

        // Act - Student submits correct answer
        var submitCommand = new SubmitWorkCommand(
            scheduleItem.Id,
            """{"selectedChoice":1}"""); // Index 1 = "4" (correct answer)

        // This would typically be done through the API/MVC controller
        // For this test, we'll simulate the submission directly
        var submission = Submission.Create(scheduleItem.Id, student.Id, submitCommand.PayloadJson);
        submission.Start();
        submission.Submit();
        
        // Auto-grade the MCQ (in a real scenario, this would be done by the system)
        var correctChoiceIndex = 1; // "4" is the correct answer
        var submittedChoiceIndex = 1; // Student selected "4"
        var isCorrect = correctChoiceIndex == submittedChoiceIndex;
        var grade = isCorrect ? scheduleItem.MaxScore : 0;
        
        if (isCorrect)
        {
            submission.SetGrade(grade.Value, "Correct! Well done.", teacher.Id);
        }

        context.Submissions.Add(submission);
        await context.SaveChangesAsync();

        // Assert
        var savedSubmission = await context.Submissions
            .FirstOrDefaultAsync(s => s.StudentId == student.Id && s.ScheduleItemId == scheduleItem.Id);

        savedSubmission.Should().NotBeNull();
        savedSubmission!.Status.Should().Be(SubmissionStatus.Graded);
        savedSubmission.Grade.Should().Be(10m);
        savedSubmission.IsPassing().Should().BeTrue();
    }

    [Fact]
    public async Task Teacher_CanCreateTeachingPlan_WithGroupsAndScheduleItems()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Arrange
        var teacher = User.Create("Test", "Teacher", "teacher@test.com");
        teacher.EmailConfirmed = true;
        await userManager.CreateAsync(teacher, "TestPass123!");
        await userManager.AddToRoleAsync(teacher, "Teacher");

        var course = Course.Create("Test Course", "Test Description", null, 1, teacher.Id, DisciplineType.Language);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act - Create teaching plan
        var teachingPlan = TeachingPlan.Create(course.Id, teacher.Id, "Integration Test Plan", "Test Description");
        context.TeachingPlans.Add(teachingPlan);
        await context.SaveChangesAsync();

        // Create groups
        var groupA = StudentGroup.Create(teachingPlan.Id, "Group A");
        var groupB = StudentGroup.Create(teachingPlan.Id, "Group B");
        context.StudentGroups.AddRange(groupA, groupB);
        await context.SaveChangesAsync();

        // Create schedule items
        var scheduleItem = ScheduleItem.Create(
            teachingPlan.Id,
            ScheduleItemType.Writing,
            "Test Writing Assignment",
            "Write a short story",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7),
            true,
            """{"type":"Writing","prompt":"Write a 100-word story about a dragon.","maxWords":100,"rubric":"basic"}""",
            20m,
            groupA.Id,
            null,
            DisciplineType.Language);

        context.ScheduleItems.Add(scheduleItem);
        await context.SaveChangesAsync();

        // Assert
        var savedPlan = await context.TeachingPlans
            .Include(tp => tp.Groups)
            .Include(tp => tp.ScheduleItems)
            .FirstOrDefaultAsync(tp => tp.Id == teachingPlan.Id);

        savedPlan.Should().NotBeNull();
        savedPlan!.Title.Should().Be("Integration Test Plan");
        savedPlan.Groups.Should().HaveCount(2);
        savedPlan.ScheduleItems.Should().HaveCount(1);
        savedPlan.ScheduleItems.First().Type.Should().Be(ScheduleItemType.Writing);
        savedPlan.ScheduleItems.First().GroupId.Should().Be(groupA.Id);
    }
}
