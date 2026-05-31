using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTO;

public class ResendEmailDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
}