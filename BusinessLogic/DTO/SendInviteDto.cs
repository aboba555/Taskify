using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTO;

public class SendInviteDto
{
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}