using Application.Features.ToDos.Handlers;
using Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.IntegrationTests;

// Integration tests for SetToDoPercentHandler
// These tests use EF Core's in-memory database to simulate real database behavior.
public class SetToDoPercentHandlerTests
{
    // Helper method to create a new in-memory ApplicationDbContext
    // Each test gets its own isolated database
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ensures isolation between tests
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenToDoNotExists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var handler = new SetToDoPercentHandler(context);

        var command = new SetToDoPercentCommand(Guid.NewGuid(), 50);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        // Expect an error result, because the ToDo does not exist
        Assert.True(result.IsError);

        // Check that the specific error code matches the "not found" scenario
        Assert.Equal("ToDo.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePercent_AndSave()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        // Insert a ToDo entity to simulate existing data
        var existing = new Application.Common.Models.ToDo
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            PercentComplete = 10
        };

        context.ToDos.Add(existing);
        await context.SaveChangesAsync();

        var handler = new SetToDoPercentHandler(context);
        var command = new SetToDoPercentCommand(existing.Id, 90);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        // Expect a success result (no errors)
        Assert.False(result.IsError);

        // Retrieve the updated entity from the in-memory database
        var updated = await context.ToDos.FirstOrDefaultAsync(t => t.Id == existing.Id);

        // Ensure it still exists and was updated correctly
        Assert.NotNull(updated);
        Assert.Equal(90, updated!.PercentComplete);
    }
}
