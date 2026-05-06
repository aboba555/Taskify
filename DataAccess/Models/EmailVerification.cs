namespace DataAccess.Models;

public class EmailVerification
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
    
    public string Code { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}