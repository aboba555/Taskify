namespace DataAccess.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    public List<TeamUser> TeamUsers { get; set; }
}