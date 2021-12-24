using System.ComponentModel.DataAnnotations;

namespace API.DTOs;
public class RegisterDTO
{
    public RegisterDTO()
    {
        Username = string.Empty;
        Password = string.Empty;
    }

    [Required]
    public string Username { get; set; }

    [Required]
    public string KnowAs { get; set; }

    [Required]
    public string Gender { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string City { get; set; }
    
    [Required]
    public string Country { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 4)]
    public string Password { get; set; }
}
