namespace BusinessLogic.DTO;

public class InvitationDto
{
    public int Id { get; set; }
    public string TeamName { get; set; }
    public string SenderName { get; set; }
    public DateTime SendedAt { get; set; }
}