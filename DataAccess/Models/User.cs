using DataAccess.Enums;

namespace DataAccess.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public string? GoogleId { get; set; }
    public string? TelegramChatId { get; set; }
    
    public List<TeamUser> TeamUsers { get; set; } = new List<TeamUser>();
}