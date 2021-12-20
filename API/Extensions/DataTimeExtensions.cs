namespace API.Extensions
{
  public static class DataTimeExtensions
  {
    public static int CalculateAge(this DateTime dateOfBirth)
    {
      var today = DateTime.Today;
      var age = today.Year - dateOfBirth.Year;
      if (dateOfBirth > today.AddYears(-age)) age--;
      return age;
    }
  }
}
