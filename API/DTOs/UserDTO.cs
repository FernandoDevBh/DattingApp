namespace API.DTOs
{
  public class UserDTO
  {
    public UserDTO()
    {
      Username =  string.Empty;
      Token = string.Empty; 
    }

    public string Username { get; set; }
    public string Token { get; set; }
  }
}
