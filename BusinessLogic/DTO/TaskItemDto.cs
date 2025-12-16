namespace BusinessLogic.DTO;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    
    public string TeamName { get; set; }
    public string AuthorName { get; set; }
    public string? AssigneeName { get; set; }
    
    public string Status { get; set; }
    public string Priority { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}