using System.Threading.Channels;
using BusinessLogic.DTO;
using BusinessLogic.Services.NotificationService;
using BusinessLogic.Services.TelegramService;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.TaskService;

public class TaskService(AppDbContext dbContext, ITelegramService telegramService, Channel<Notification> channelNotification) : ITaskService
{
    public async Task CreateTask(CreateTaskDto createTaskDto, int userId)
    {
        bool validatedUser = await dbContext.TeamUsers.AnyAsync(x => x.UserId == userId && x.TeamId == createTaskDto.TeamId);
        if (!validatedUser)
        {
            throw new Exception("User is not part of this team");
        }
        
        if (createTaskDto.AssignedToUserId != null) 
        {
            bool validateAssigned = await dbContext.TeamUsers
                .AnyAsync(x => x.UserId == createTaskDto.AssignedToUserId && x.TeamId == createTaskDto.TeamId);
            
            if (!validateAssigned)
            {
                throw new Exception("Assigned user is not part of this team");
            }
        }

        var createdTask = new TaskItem()
        {
            Label = createTaskDto.Label,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            TeamId = createTaskDto.TeamId,
            AssignedToUserId = createTaskDto.AssignedToUserId,
            Status = ItemStatus.New,
            Priority = createTaskDto.Priority,
            DueDate = createTaskDto.DueDate,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId,
        };
        
        await dbContext.TaskItems.AddAsync(createdTask);
        await dbContext.SaveChangesAsync();
        
        if (createTaskDto.AssignedToUserId != null && createTaskDto.AssignedToUserId != userId)
        {
            var assigner = await dbContext.Users.FindAsync(userId);
            var message = $"📋 *New task assigned to you!*\n\n" +
                          $"📌 {createdTask.Title}\n" +
                          $"👤 From: {assigner.FirstName} {assigner.LastName}\n" +
                          $"📅 Due: {createTaskDto.DueDate?.ToString("dd MMM yyyy") ?? "No deadline"}\n" +
                          $"🔥 Priority: {createTaskDto.Priority}";
        
            await telegramService.SendMessage(createTaskDto.AssignedToUserId.Value, message);
        }
    }

    public async Task UpdateTask(UpdateTaskDto updateTaskDto, int userId)
    {
        var task = await dbContext.TaskItems.FindAsync(updateTaskDto.TaskId);
        if (task == null)
        {
            throw new Exception(message: "Task not found");
        }

        bool hasAccess = await dbContext.TeamUsers
            .AnyAsync(x => x.UserId == userId && x.TeamId == task.TeamId);

        if (!hasAccess)
        {
            throw new Exception("Access denied");
        }
        
        var oldAssigneeId = task.AssignedToUserId;
        
        if (updateTaskDto.AssignedToUserId != null)
        {
            bool validateAssigned = await dbContext.TeamUsers.AnyAsync(x =>
                x.TeamId == task.TeamId && x.UserId == updateTaskDto.AssignedToUserId);

            if (!validateAssigned)
                throw new Exception("Assigned user is not part of this team");
        }
        
        if (!string.IsNullOrEmpty(updateTaskDto.Title))
        {
            task.Title = updateTaskDto.Title;
        }

        if (!string.IsNullOrEmpty(updateTaskDto.Description))
        {
            task.Description = updateTaskDto.Description;
        }
        
        task.Label = updateTaskDto.Label;
        task.Status = updateTaskDto.Status;
        task.AssignedToUserId = updateTaskDto.AssignedToUserId;
        task.UpdatedAt = DateTime.UtcNow; 
        
        await dbContext.SaveChangesAsync();
        
        if (updateTaskDto.AssignedToUserId != null && 
            updateTaskDto.AssignedToUserId != oldAssigneeId &&
            updateTaskDto.AssignedToUserId != userId)
        {
            var assigner = await dbContext.Users.FindAsync(userId);
            var message = $"📋 *Task reassigned to you!*\n\n" +
                          $"📌 {task.Title}\n" +
                          $"👤 From: {assigner.FirstName} {assigner.LastName}";
        
            await telegramService.SendMessage(updateTaskDto.AssignedToUserId.Value, message);
        }
    }

    public async Task AddComment(CreateCommentDto createCommentDto, int userId)
    {
        var task = await dbContext.TaskItems.FindAsync(createCommentDto.TaskId);
        if (task == null)
        {
            throw new Exception("Task not found");
        }
        
        bool hasAccess = await dbContext.TeamUsers
            .AnyAsync(tu => tu.UserId == userId && tu.TeamId == task.TeamId);

        if (!hasAccess)
        {
            throw new Exception("Access denied: You are not in this team");
        }
        
        var newComment = new Comment
        {
            Text = createCommentDto.Text,
            TaskId = createCommentDto.TaskId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var validMentions = await dbContext.TeamUsers
            .Where(t => t.TeamId == task.TeamId 
                        && createCommentDto.MentionedUserIds.Contains(t.UserId) 
                        && t.UserId != userId)
            .Select(t => t.UserId)
            .ToListAsync();
        
        var author = await dbContext.Users.FindAsync(userId);

        foreach (var mentionedUserId in validMentions)
        {
            var notification = new Notification
            {
                FromUserId = userId,
                ToUserId = mentionedUserId,
                TaskId = task.Id,
                Text = $"{author.FirstName} mentioned you in task: {task.Title}",
                Type = NotificationType.MENTION,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            await channelNotification.Writer.WriteAsync(notification);
        }


        await dbContext.Comments.AddAsync(newComment);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<TaskItemDto>> GetTeamTasks(int teamId, int userId)
    {
        bool hasAccess = await dbContext.TeamUsers.AnyAsync(x => x.UserId == userId && x.TeamId == teamId);
        if (!hasAccess)
        {
            throw new Exception("Access denied");
        }

        return await dbContext.TaskItems
            .AsNoTracking()
            .Where(x => x.TeamId == teamId)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Label = t.Label,
                Title = t.Title,
                Description = t.Description != null ? t.Description : string.Empty,
                TeamName = t.Team.Name,
                AuthorName = $"{t.CreatedByUser.FirstName} {t.CreatedByUser.LastName}",
                AssigneeName = t.AssignedToUser != null
                    ? $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}"
                    : "Unassigned",
                AssignedToUserId = t.AssignedToUserId,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate
            })
            .ToListAsync();
    }

    public async Task<List<TaskItemDto>> GetMyTask(int userId)
    {
        return await dbContext.TaskItems
            .AsNoTracking()
            .Where(t => t.AssignedToUserId == userId)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Label = t.Label,
                Title = t.Title,
                Description = t.Description != null ? t.Description : string.Empty,
                TeamName = t.Team.Name,
                AuthorName = $"{t.CreatedByUser.FirstName} {t.CreatedByUser.LastName}",
                AssigneeName = t.AssignedToUser != null 
                    ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName 
                    : "Unknown",
                AssignedToUserId = t.AssignedToUserId,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate
            })
            .ToListAsync();
    }

    public async Task<List<CommentDto>> GetCommentsByTaskId(int taskId, int userId)
    {
        var task = await dbContext.TaskItems.FindAsync(taskId);
        if (task == null)
        {
            throw new Exception("Task not found");
        }
        
        bool hasAccess = await dbContext.TeamUsers
            .AnyAsync(x => x.UserId == userId && x.TeamId == task.TeamId);
    
        if (!hasAccess)
        {
            throw new Exception("Access denied");
        }
    
        return await dbContext.Comments
            .Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new CommentDto
            {
                Id = x.Id,
                TaskId = taskId,
                CreatedByUser = $"{x.CreatedByUser.FirstName} {x.CreatedByUser.LastName}",
                Text = x.Text,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task DeleteTask(int taskId, int userId)
    {
        var task = await dbContext.TaskItems.FindAsync(taskId);
        if (task == null)
        {
            throw new Exception("Task not found");
        }
        
        bool hasAcces = await dbContext.TeamUsers.AnyAsync(x=> x.UserId == userId && x.TeamId == task.TeamId);
        if (!hasAcces)
        {
            throw new Exception("Access denied");
        }
        dbContext.TaskItems.Remove(task);
        await dbContext.SaveChangesAsync();
    }
}