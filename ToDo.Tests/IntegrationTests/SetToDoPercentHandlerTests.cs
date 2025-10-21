using Application.Features.ToDos.Handlers;
using Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.IntegrationTests;

public class SetToDoPercentHandlerTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
        Assert.True(result.IsError);
        Assert.Equal("ToDo.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePercent_AndSave()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

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
        Assert.False(result.IsError);

        var updated = await context.ToDos.FirstOrDefaultAsync(t => t.Id == existing.Id);
        Assert.NotNull(updated);
        Assert.Equal(90, updated!.PercentComplete);
    }
}