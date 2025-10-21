using Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Features.ToDos.Handlers;
using Application.Common.Models;
using Moq;
using Xunit;

namespace ToDo.Tests.UnitTests;

public class CreateToDoHandlerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Application.Common.Models.ToDo>> _mockSet;
    private readonly CreateToDoHandler _handler;

    public CreateToDoHandlerTests()
    {
        _mockSet = new Mock<DbSet<Application.Common.Models.ToDo>>();
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockContext.Setup(m => m.ToDos).Returns(_mockSet.Object);

        _handler = new CreateToDoHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddNewToDo_AndSaveChanges()
    {
        // Arrange
        var command = new CreateToDoCommand
        {
            Title = "Mocked ToDo",
            Description = "Testing with Moq",
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockSet.Verify(m => m.Add(It.IsAny<Application.Common.Models.ToDo>()), Times.Once);
        _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsError);
        Assert.Equal("Mocked ToDo", result.Value.Title);
    }
}