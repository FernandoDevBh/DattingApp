namespace API.Extensions
{
  public static class CorsServiceExtensions
  {
    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
      var MyAllowSpecificOrigins = "MyAllowSpecificOrigins";
      services.AddCors(options =>
      {
        options.AddPolicy(name: MyAllowSpecificOrigins, builder =>
        {
          builder.WithOrigins("https://localhost:4200", "http://localhost:4200/").AllowAnyHeader().AllowAnyMethod();
        });
      });
      return services;
    }
  }
}
