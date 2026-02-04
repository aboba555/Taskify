namespace DataAccess.Models;

public class TelegramLinkCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}