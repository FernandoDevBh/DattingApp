using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
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
    [StringLength(8, MinimumLength = 4)]
    public string Password { get; set; }
  }
}
