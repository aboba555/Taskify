using DataAccess.Enums;
using DataAccess.Models;

namespace BusinessLogic.Services.NotificationService;

public interface INotificationService
{
    Task SendNotificationAsync(int userId, string message, int taskId, NotificationType notificationType);
}