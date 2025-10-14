using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EduTrack.Infrastructure.Data;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using AutoFixture;
using FluentAssertions;

namespace EduTrack.TestUtilities;

/// <summary>
/// Base class for integration tests with common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly AppDbContext Context;
    protected readonly Fixture Fixture;
    protected readonly IServiceScope Scope;

    protected TestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add InMemory database for testing
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    });
                });
            });

        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Fixture = new Fixture();
        
        // Configure AutoFixture to handle circular references
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Creates a test user with specified role
    /// </summary>
    protected async Task<User> CreateTestUserAsync(string? email = null, string? firstName = null, string? lastName = null)
    {
        var user = User.Create(
            email ?? Fixture.Create<string>() + "@test.com",
            firstName ?? Fixture.Create<string>(),
            lastName ?? Fixture.Create<string>()
        );
        
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Creates a test course
    /// </summary>
    protected async Task<Course> CreateTestCourseAsync(string? title = null, DisciplineType? disciplineType = null)
    {
        var course = Course.Create(
            title ?? Fixture.Create<string>(),
            Fixture.Create<string>(),
            "test-thumbnail.jpg",
            Fixture.Create<int>(),
            "test-user",
            disciplineType ?? Fixture.Create<DisciplineType>()
        );
        
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        return course;
    }

    /// <summary>
    /// Creates a test class
    /// </summary>
    protected async Task<Class> CreateTestClassAsync(int courseId, string? name = null)
    {
        var classEntity = Class.Create(
            courseId,
            name ?? Fixture.Create<string>(),
            Fixture.Create<string>(),
            "test-teacher",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30)
        );
        
        Context.Classes.Add(classEntity);
        await Context.SaveChangesAsync();
        return classEntity;
    }

    /// <summary>
    /// Creates a test teaching plan
    /// </summary>
    protected async Task<TeachingPlan> CreateTestTeachingPlanAsync(int courseId, string? title = null)
    {
        var teachingPlan = TeachingPlan.Create(
            courseId,
            "test-teacher",
            title ?? Fixture.Create<string>(),
            Fixture.Create<string>()
        );
        
        Context.TeachingPlans.Add(teachingPlan);
        await Context.SaveChangesAsync();
        return teachingPlan;
    }

    /// <summary>
    /// Creates a test schedule item
    /// </summary>
    protected async Task<ScheduleItem> CreateTestScheduleItemAsync(int teachingPlanId, ScheduleItemType? type = null)
    {
        var scheduleItem = ScheduleItem.Create(
            teachingPlanId,
            type ?? Fixture.Create<ScheduleItemType>(),
            Fixture.Create<string>(),
            Fixture.Create<string>(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7),
            false,
            "{}",
            null,
            null,
            null,
            null
        );
        
        Context.ScheduleItems.Add(scheduleItem);
        await Context.SaveChangesAsync();
        return scheduleItem;
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        Context.Set<ScheduleItemStudentAssignment>().RemoveRange(Context.Set<ScheduleItemStudentAssignment>());
        Context.Set<ScheduleItemSubChapterAssignment>().RemoveRange(Context.Set<ScheduleItemSubChapterAssignment>());
        Context.Set<ScheduleItemGroupAssignment>().RemoveRange(Context.Set<ScheduleItemGroupAssignment>());
        Context.ScheduleItems.RemoveRange(Context.ScheduleItems);
        Context.TeachingPlans.RemoveRange(Context.TeachingPlans);
        Context.Classes.RemoveRange(Context.Classes);
        Context.Courses.RemoveRange(Context.Courses);
        Context.Users.RemoveRange(Context.Users);
        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Asserts that a response is successful
    /// </summary>
    protected void AssertSuccessResponse(HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {response.StatusCode}: {response.ReasonPhrase}");
    }

    /// <summary>
    /// Asserts that a response has a specific status code
    /// </summary>
    protected void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode)
    {
        response.StatusCode.Should().Be(expectedStatusCode,
            $"Expected {expectedStatusCode} but got {response.StatusCode}: {response.ReasonPhrase}");
    }

    public virtual void Dispose()
    {
        Scope?.Dispose();
        Client?.Dispose();
        Factory?.Dispose();
        Context?.Dispose();
    }
}
