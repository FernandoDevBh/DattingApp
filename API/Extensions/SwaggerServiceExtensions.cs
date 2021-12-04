using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
  public static class SwaggerServiceExtensions
  {
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen();      
      return services;
    }
  }
}
