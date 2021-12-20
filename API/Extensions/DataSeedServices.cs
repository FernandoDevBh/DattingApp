using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
  public static class DataSeedServices
  {
    public static async Task SeedData(this IServiceProvider provider)
    {
      using var scope = provider.CreateScope();
      var services = scope.ServiceProvider;
      try
      {
        var context = services.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
        await Seed.SeedUsers(context);
      }
      catch (Exception ex)
      {
        var logger = services.GetRequiredService<ILogger>();
        logger.LogError(ex, "An error occurred during migration");
      }
    }
  }
}
