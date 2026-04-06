using BusinessLogic.Services.TelegramService;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.NotificationService;

public class NotificationService(ITelegramService telegramService, AppDbContext dbContext) : INotificationService
{
    public async Task SendNotificationAsync(int userId, string message, int taskId, NotificationType notificationType)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var task = await dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);

        if (user == null || task == null)
            throw new Exception("User or task not found");
        
        if (user.TelegramChatId != null)
        {
            await telegramService.SendMessage(user.Id, message);
        }
        
        var notification = new Notification
        {
            ToUserId = userId,
            TaskId = taskId,
            Text = message,
            Type = notificationType,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        await dbContext.Notifications.AddAsync(notification);
        await dbContext.SaveChangesAsync();
    }
}