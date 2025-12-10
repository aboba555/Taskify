using DataAccess.Enums;

namespace BusinessLogic.DTO;

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Role { get; set; }
}