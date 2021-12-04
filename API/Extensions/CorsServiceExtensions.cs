namespace API.Extensions
{
  public static class CorsServiceExtensions
  {
    public static string GetCorsPolicyName(this IServiceCollection services) => "MyAllowSpecificOrigins";
    public static IServiceCollection AddCorsServices(this IServiceCollection services)
   {
      services.AddCors(options =>
      {
        options.AddPolicy(name: services.GetCorsPolicyName(), builder =>
        {
          builder.WithOrigins("https://localhost:4200", "http://localhost:4200/").AllowAnyHeader().AllowAnyMethod();
        });
      });
      return services;
    }
  }
}
