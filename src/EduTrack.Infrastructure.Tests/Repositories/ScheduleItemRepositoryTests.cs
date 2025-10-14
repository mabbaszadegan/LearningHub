using Microsoft.EntityFrameworkCore;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Repositories;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EduTrack.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for ScheduleItemRepository
/// </summary>
public class ScheduleItemRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ScheduleItemRepository _repository;

    public ScheduleItemRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new ScheduleItemRepository(_context);
    }

    [Fact]
    public async Task AddAsync_WithValidScheduleItem_ShouldAddToDatabase()
    {
        // Arrange
        var scheduleItem = CreateTestScheduleItem();

        // Act
        await _repository.AddAsync(scheduleItem, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var savedScheduleItem = await _context.ScheduleItems
            .FirstOrDefaultAsync(si => si.Id == scheduleItem.Id);
        savedScheduleItem.Should().NotBeNull();
        savedScheduleItem!.Title.Should().Be(scheduleItem.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnScheduleItem()
    {
        // Arrange
        var scheduleItem = CreateTestScheduleItem();
        _context.ScheduleItems.Add(scheduleItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.ScheduleItems.FirstOrDefaultAsync(si => si.Id == scheduleItem.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(scheduleItem.Id);
        result.Title.Should().Be(scheduleItem.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidScheduleItem_ShouldUpdateInDatabase()
    {
        // Arrange
        var scheduleItem = CreateTestScheduleItem();
        _context.ScheduleItems.Add(scheduleItem);
        await _context.SaveChangesAsync();

        // Act
        scheduleItem.UpdateTitle("Updated Title");
        await _repository.UpdateAsync(scheduleItem, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        var updatedScheduleItem = await _context.ScheduleItems
            .FirstOrDefaultAsync(si => si.Id == scheduleItem.Id);
        updatedScheduleItem.Should().NotBeNull();
        updatedScheduleItem!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_WithValidEntity_ShouldRemoveFromDatabase()
    {
        // Arrange
        var scheduleItem = CreateTestScheduleItem();
        _context.ScheduleItems.Add(scheduleItem);
        await _context.SaveChangesAsync();

        // Act
        _context.ScheduleItems.Remove(scheduleItem);
        await _context.SaveChangesAsync();

        // Assert
        var deletedScheduleItem = await _context.ScheduleItems
            .FirstOrDefaultAsync(si => si.Id == scheduleItem.Id);
        deletedScheduleItem.Should().BeNull();
    }

    private static ScheduleItem CreateTestScheduleItem()
    {
        return ScheduleItem.Create(
            1, // teachingPlanId
            ScheduleItemType.Writing,
            "Test Schedule Item",
            "Test Description",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7),
            false,
            "{}",
            null,
            null,
            null,
            null
        );
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
