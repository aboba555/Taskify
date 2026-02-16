using BusinessLogic.DTO;
using BusinessLogic.Services.TaskService;
using BusinessLogic.Services.TelegramService;
using DataAccess;
using DataAccess.Models;
using FluentAssertions;
using Google.Apis.Util;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Taskify.Tests;

public class TaskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ITelegramService> _telegramMock;
    private readonly TaskService _sut;
    
    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName:Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _telegramMock = new Mock<ITelegramService>();
        _sut = new TaskService(_context, _telegramMock.Object);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    // -------------
    // CREATE TASK TESTS 
    // --------------
    
    [Fact]
    public async Task CreateTask_UserNotInTeam_ThrowsException()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Test",
            TeamId = 1
        };
        
        // Act
        var act = () => _sut.CreateTask(dto, userId: 1);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateTask_UserInTeam_CreatesTask()
    { 
        // Arrange
        var userId = 1;
        _context.TeamUsers.Add(new TeamUser { UserId = userId, TeamId = 5 });
        await _context.SaveChangesAsync();
        
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test",
            TeamId = 5,
        };
        
        // Act
        await _sut.CreateTask(createTaskDto, userId);
        
        // Assert
        var task = await _context.TaskItems.FirstOrDefaultAsync();
        task.Should().NotBeNull();
        task.Title.Should().Be("Test");
        task.TeamId.Should().Be(5);
    }

    [Fact]
    public async Task CreateTask_AssignedNotInTeam_ThrowsException()
    {
        // Arrange
        var creatorUserId = 1;
        _context.TeamUsers.Add(new TeamUser { UserId = creatorUserId, TeamId = 5 });
        await _context.SaveChangesAsync();
        
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test",
            TeamId = 5,
            AssignedToUserId = 3
        };
        
        // Act
        var act = () => _sut.CreateTask(createTaskDto, creatorUserId);
        
        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task CreateTask_AssignedInTeam_CreatesTask()
    {
        // Arrange
        var creatorUserId = 1;
        var assignedToUserId = 3;
        _context.Users.Add(new User { Id = creatorUserId, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
        _context.Users.Add(new User { Id = assignedToUserId, FirstName = "Jane", LastName = "Doe", Email = "bob2@gmail.com" });
        _context.TeamUsers.Add(new TeamUser { UserId = creatorUserId, TeamId = 5 });
        _context.TeamUsers.Add(new TeamUser { UserId = assignedToUserId, TeamId = 5 });
        await _context.SaveChangesAsync();
        
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test",
            TeamId = 5,
            AssignedToUserId = assignedToUserId
        };
        
        // Act
        await _sut.CreateTask(createTaskDto, creatorUserId);
        
        // Assert
        var task = await _context.TaskItems.FirstOrDefaultAsync();
        task.Should().NotBeNull();
        task.Title.Should().Be("Test");
        task.TeamId.Should().Be(5);
        task.AssignedToUserId.Should().Be(assignedToUserId);
    }

    [Fact]
    public async Task CreateTask_AssignedToYourself_NoTelegramNotificitation()
    {
        // Arrange
        var creatorUserId = 1;
        _context.Users.Add(new User { Id = creatorUserId, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
        _context.TeamUsers.Add(new TeamUser { UserId = creatorUserId, TeamId = 5 });
        await _context.SaveChangesAsync();

        var createTaskDto = new CreateTaskDto
        {
            Title = "Test",
            TeamId = 5,
            AssignedToUserId = creatorUserId
        };
        
        // Act
        await _sut.CreateTask(createTaskDto, creatorUserId);
        
        // Assert
        _telegramMock.Verify(
            t => t.SendMessage(It.IsAny<int>(), It.IsAny<string>()), Times.Never
            );
    }
    
    [Fact]
    public async Task CreateTask_AssignedToNotYourself_SendsTelegramNotificitation()
    {
        // Arrange
        var creatorUserId = 1;
        var assigneedUserId = 2;
        _context.Users.Add(new User { Id = creatorUserId, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
        _context.Users.Add(new User { Id = assigneedUserId, FirstName = "Bobik", LastName = "Dorebloa", Email = "bob2@gmail.com"});
        _context.TeamUsers.Add(new TeamUser { UserId = creatorUserId, TeamId = 5 });
        _context.TeamUsers.Add(new TeamUser { UserId = assigneedUserId, TeamId = 5 });
        await _context.SaveChangesAsync();

        var createTaskDto = new CreateTaskDto
        {
            Title = "Test",
            TeamId = 5,
            AssignedToUserId = assigneedUserId
        };
        
        // Act
        await _sut.CreateTask(createTaskDto, creatorUserId);
        
        // Assert
        _telegramMock.Verify(
            t => t.SendMessage(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(1)
        );
    }
    
    // --------------
    // UPDATE TASK TESTS 
    // --------------
   
	
}