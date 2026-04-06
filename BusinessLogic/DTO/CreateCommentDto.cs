namespace BusinessLogic.DTO;

public class CreateCommentDto 
{
    public int TaskId { get; set; }
    public string Text { get; set; }
    public List<int> MentionedUserIds { get; set; } = new();
}