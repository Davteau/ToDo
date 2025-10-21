using Application.Features.ToDos.Handlers;
using Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ToDo.Tests.UnitTests;

// Unit tests for the CreateToDoHandler class.
public class CreateToDoHandlerTests
{
    // Mocked dependencies for isolation from real database
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<DbSet<Application.Common.Models.ToDo>> _mockSet;
    private readonly CreateToDoHandler _handler;

    public CreateToDoHandlerTests()
    {
        // Create a mock DbSet to simulate EF Core ToDos table
        _mockSet = new Mock<DbSet<Application.Common.Models.ToDo>>();

        // Create a mock DbContext with fake options
        _mockContext = new Mock<ApplicationDbContext>(
            new DbContextOptions<ApplicationDbContext>()
        );

        // Configure the DbContext mock to return our mock DbSet when ToDos is accessed
        _mockContext.Setup(m => m.ToDos).Returns(_mockSet.Object);

        // Initialize the handler with the mocked context
        _handler = new CreateToDoHandler(_mockContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddNewToDo_AndSaveChanges()
    {
        // Arrange
        // Prepare a sample command that represents user input for creating a ToDo item
        var command = new CreateToDoCommand
        {
            Title = "Mocked ToDo",
            Description = "Testing with Moq",
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify that a new ToDo entity was added to the DbSet
        _mockSet.Verify(
            m => m.Add(It.IsAny<Application.Common.Models.ToDo>()),
            Times.Once
        );

        // Verify that SaveChangesAsync was called once to persist changes
        _mockContext.Verify(
            m => m.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Ensure that the result was successful (no errors)
        Assert.False(result.IsError);

        // Verify that the returned ToDo contains the expected title
        Assert.Equal("Mocked ToDo", result.Value.Title);
    }
}
