using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTO;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Code is required")]
    public string Code { get; set; }
}