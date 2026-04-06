using DataAccess.Enums;

namespace DataAccess.Models;

public class Notification
{
    public int Id { get; set; }
    
    public string Text { get; set; }
    
    public int? FromUserId { get; set; }
    public User? FromUser { get; set; }
    
    public int ToUserId { get; set; }
    public User ToUser { get; set; }
    
    public int? TaskId { get; set; }
    public TaskItem? TaskItem { get; set; }
    
    public NotificationType Type { get; set; }
    
    public bool IsRead { get; set; }
    
    public DateTime SentAt { get; set; }
}