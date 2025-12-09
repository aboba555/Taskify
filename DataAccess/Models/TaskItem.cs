using DataAccess.Enums;

namespace DataAccess.Models;

public class TaskItem
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    public string? Description { get; set; }
    
    public int TeamId { get; set; }
    public Team Team { get; set; }
    
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    
    public ItemStatus Status { get; set; }
    public Priority Priority { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; }
    
    public List<Comment> Comments { get; set; } = new List<Comment>();
    
}