namespace API.DTOs
{
  public class LoginDTO
  {
    public LoginDTO()
    {
      Username =  string.Empty;
      Password = string.Empty; 
    }

    public string Username { get; set; }
    public string Password { get; set; }
  }
}
