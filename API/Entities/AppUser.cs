namespace API.Entities
{
  public class AppUser
  {
    public AppUser()
    {
      Id = 0;
      UserName = String.Empty;
      PasswordSalt = new byte[] { };
      PasswordHash = new byte[] { };
    }

    public int Id { get; set; }
    public string UserName { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
  }
}
