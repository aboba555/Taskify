using DataAccess.Enums;

namespace DataAccess.Models;

public class Invitation
{
    public int Id { get; set; }
    
    public int TeamId { get; set; }
    public Team Team { get; set; }
    
    public string Email { get; set; }
    
    public int? InvitedUserId { get; set; }
    public User? InvitedUser { get; set; }
    
    public int SenderId { get; set; }
    public User? Sender { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public Status Status { get; set; } = Status.Pending;
}