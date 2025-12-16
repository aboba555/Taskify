using DataAccess.Enums;

namespace BusinessLogic.DTO;

public class UpdateTaskDto
{
    public int TaskId { get; set; }
    public int TeamId { get; set; }
    public ItemStatus Status { get; set; }
    public int? AssignedToUserId { get; set; }
}