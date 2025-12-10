using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTO;


public class RegisterUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Firstname is required")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Lastname is required")]
    public string LastName { get; set; }
}