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
    public string Password { get; set; }
  }
}
