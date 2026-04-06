using BusinessLogic.Services.TelegramService;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.NotificationService;

public class NotificationService(ITelegramService telegramService, AppDbContext dbContext) : INotificationService
{
    public async Task<IEnumerable<Notification>> GetAllNotificationsByUserId(int userId)
    {
        var notifications =  await dbContext.Notifications.Where(u => u.ToUserId == userId).ToListAsync();
        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetAllUnReadNotificationsByUserId(int userId)
    {
        var notifications = await dbContext.Notifications.Where(u => u.ToUserId == userId && !u.IsRead).ToListAsync();
        return notifications;
    }

    public async Task MarkOneAsRead(int notificationId, int userId)
    {
        var notification = await dbContext.Notifications.FindAsync(notificationId);
        
        if (notification == null)
            throw new Exception("Notification not found");
        
        if (notification.ToUserId != userId)
        {
            throw new ArgumentException($"This notification doesn't belong to user {userId}");
        }
        notification.IsRead = true;
        await dbContext.SaveChangesAsync();
    }

    public async Task MarkAllAsRead(int userId)
    {
        var notification = await dbContext.Notifications.Where(u => u.ToUserId == userId && !u.IsRead).ToListAsync();
        notification.ForEach(n => n.IsRead = true);
        await dbContext.SaveChangesAsync();
    }
    public async Task<int> GetUnreadCount(int userId)
    {
        return await dbContext.Notifications.CountAsync(n => n.ToUserId == userId && !n.IsRead);
    }
}