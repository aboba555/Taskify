namespace BusinessLogic.DTO;

public class CommentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string CreatedByUser { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; } 
}