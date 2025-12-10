using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTO;

public class CreateTeamDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
    public string Name { get; set; }
    
    public string? Description { get; set; }
}