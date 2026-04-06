using DataAccess.Enums;
using DataAccess.Models;

namespace BusinessLogic.Services.NotificationService;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetAllNotificationsByUserId(int userId);
    Task<IEnumerable<Notification>> GetAllUnReadNotificationsByUserId(int userId);
    Task MarkOneAsRead(int notificationId, int userId);
    Task MarkAllAsRead(int userId);
    Task<int> GetUnreadCount(int userId);
}