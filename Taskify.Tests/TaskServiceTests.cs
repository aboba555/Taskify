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
    
   [Fact]
   public async Task UpdateTask_NotValidTaskId_ThrowsException()
   {
       // Arrange
       var updateTaskDto = new UpdateTaskDto
       {
           Title = "Test2",
           TeamId = 9999,
           TaskId = 555
       };
       // Act
       var act = () => _sut.UpdateTask(updateTaskDto, userId: 1);

       // Assert
       await act.Should().ThrowAsync<Exception>()
           .WithMessage("Task not found");
   }

   [Fact]
   public async Task UpdateTask_WithoutAccess_ThrowsException()
   {
       var userA = 1;
       var userB = 2;
       _context.Users.Add(new User { Id = userA, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
       _context.Users.Add(new User { Id = userB, FirstName = "Bobik", LastName = "Dorebloa", Email = "bob2@gmail.com"});
       _context.TeamUsers.Add(new TeamUser { UserId = userA, TeamId = 5 });
       _context.TeamUsers.Add(new TeamUser { UserId = userB, TeamId = 6 });
       _context.TaskItems.Add(new TaskItem { Title = "Test", AssignedToUserId = userA, TeamId = 5, Id = 1});
       await _context.SaveChangesAsync();
       
       var updateTaskDto = new UpdateTaskDto
       {
           TaskId = 1,
           Title = "Test2",
           TeamId = 5,
       };
       
       var act = () => _sut.UpdateTask(updateTaskDto, userB);
       
       await act.Should().ThrowAsync<Exception>()
           .WithMessage("Access denied");
   }
	
   [Fact]
   public async Task UpdateTask_WithAccess_UpdatingTask()
   {
       var userA = 1;
       var userB = 2;
       _context.Users.Add(new User { Id = userA, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
       _context.Users.Add(new User { Id = userB, FirstName = "Bobik", LastName = "Dorebloa", Email = "bob2@gmail.com"});
       _context.TeamUsers.Add(new TeamUser { UserId = userA, TeamId = 5 });
       _context.TeamUsers.Add(new TeamUser { UserId = userB, TeamId = 6 });
       _context.TaskItems.Add(new TaskItem { Title = "Test", AssignedToUserId = userA, TeamId = 5, Id = 1});
       await _context.SaveChangesAsync();
       
       var updateTaskDto = new UpdateTaskDto
       {
           TaskId = 1,
           Title = "Test2",
           TeamId = 5,
           AssignedToUserId = userA
       };
       
       await _sut.UpdateTask(updateTaskDto, userA);
       var task = await _context.TaskItems.FirstAsync(t=> t.Id == 1);
       
       task.Should().NotBeNull();
       task.Title.Should().Be("Test2");
       task.TeamId.Should().Be(5);
       task.AssignedToUserId.Should().Be(userA);
   }

   [Fact]
   public async Task UpdateTask_AsigneeToNotValidUser_ThrowsException()
   {
       var userA = 1;
       var userB = 2;
       
       _context.Users.Add(new User { Id = userA, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
       _context.Users.Add(new User { Id = userB, FirstName = "Bobik", LastName = "Dorebloa", Email = "bob2@gmail.com"});
       _context.TeamUsers.Add(new TeamUser { UserId = userA, TeamId = 5 });
       _context.TeamUsers.Add(new TeamUser { UserId = userB, TeamId = 6 });
       _context.TaskItems.Add(new TaskItem { Title = "Test", AssignedToUserId = userA, TeamId = 5, Id = 1});
       await _context.SaveChangesAsync();

       var updateTaskDto = new UpdateTaskDto
       {
           TaskId = 1,
           Title = "Test2",
           TeamId = 5,
           AssignedToUserId = userB
       };
       
       var act = () => _sut.UpdateTask(updateTaskDto, userA);
       
       await act.Should().ThrowAsync<Exception>()
           .WithMessage("Assigned user is not part of this team");
   }
   
   [Fact]
   public async Task UpdateTask_ReAssignee_SendsNotification()
   {
       var userA = 1;
       var userB = 2;
       
       _context.Users.Add(new User { Id = userA, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
       _context.Users.Add(new User { Id = userB, FirstName = "Bobik", LastName = "Dorebloa", Email = "bob2@gmail.com"});
       
       _context.TeamUsers.Add(new TeamUser { UserId = userA, TeamId = 5 });
       _context.TeamUsers.Add(new TeamUser { UserId = userB, TeamId = 5 });
       
       _context.TaskItems.Add(new TaskItem { Title = "Test", AssignedToUserId = userA, TeamId = 5, Id = 1});
       await _context.SaveChangesAsync();

       var updateTaskDto = new UpdateTaskDto
       {
           TaskId = 1,
           Title = "Test2",
           TeamId = 5,
           AssignedToUserId = userB
       };
       
       await _sut.UpdateTask(updateTaskDto, userA);
       
       _telegramMock.Verify(t=> t.SendMessage(userB, It.IsAny<string>()), Times.Once);
           
   }
   
   [Fact]
   public async Task UpdateTask_WithTheSameUserAssigned_DontSendNotification()
   {
       var userA = 1;
       
       _context.Users.Add(new User { Id = userA, FirstName = "John", LastName = "Doe", Email = "bob1@gmail.com"});
       
       _context.TeamUsers.Add(new TeamUser { UserId = userA, TeamId = 5 });
       
       _context.TaskItems.Add(new TaskItem { Title = "Test", AssignedToUserId = userA, TeamId = 5, Id = 1});
       await _context.SaveChangesAsync();

       var updateTaskDto = new UpdateTaskDto
       {
           TaskId = 1,
           Title = "Test2",
           TeamId = 5,
           AssignedToUserId = userA
       };
       
       await _sut.UpdateTask(updateTaskDto, userA);
       
       _telegramMock.Verify(t=> t.SendMessage(userA, It.IsAny<string>()), Times.Never);
       _telegramMock.Verify(t=> t.SendMessage(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
           
   }
}