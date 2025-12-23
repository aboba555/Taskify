using DataAccess.Enums;

namespace BusinessLogic.DTO;

public class CreateTaskDto
{
    public Label Label { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int TeamId { get; set; }
    public int? AssignedToUserId { get; set; }
    public Priority Priority { get; set; }
    public DateTime? DueDate { get; set; }
}