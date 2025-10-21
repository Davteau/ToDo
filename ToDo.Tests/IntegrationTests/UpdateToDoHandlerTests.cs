using Application.Common.Models;
using Application.Features.ToDos.Handlers;
using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.IntegrationTests;

// Integration tests for the UpdateToDoHandler class.
// These tests use EF Core's in-memory database to simulate real database behavior.
public class UpdateToDoHandlerTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenToDoDoesNotExist()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var handler = new UpdateToDoHandler(context);

        // Create a command for a ToDo that doesn't exist in the DB
        var command = new UpdateToDoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Doesn't exist",
            Description = "Desc",
            ExpiryDate = DateTime.UtcNow,
            PercentComplete = 0
        };

        // Act
        ErrorOr<Unit> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        // Expect the result to indicate an error (not found)
        Assert.True(result.IsError);
        Assert.Equal("ToDo.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_ShouldUpdateFields_AndSaveChanges()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var existing = new Application.Common.Models.ToDo
        {
            Id = Guid.NewGuid(),
            Title = "Old",
            Description = "Old",
            ExpiryDate = DateTime.UtcNow,
            PercentComplete = 10
        };

        context.ToDos.Add(existing);
        await context.SaveChangesAsync();

        var handler = new UpdateToDoHandler(context);

        // Prepare an update command with new values
        var command = new UpdateToDoCommand
        {
            Id = existing.Id,
            Title = "New",
            Description = "Updated",
            ExpiryDate = DateTime.UtcNow.AddDays(2),
            PercentComplete = 50
        };

        // Act
        ErrorOr<Unit> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        // Expect no error
        Assert.False(result.IsError);

        // Fetch the updated entity from the database
        var updated = await context.ToDos.FirstOrDefaultAsync(t => t.Id == existing.Id);

        // Verify it still exists and all fields were updated correctly
        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Title);
        Assert.Equal("Updated", updated.Description);
        Assert.Equal(50, updated.PercentComplete);
    }
}
