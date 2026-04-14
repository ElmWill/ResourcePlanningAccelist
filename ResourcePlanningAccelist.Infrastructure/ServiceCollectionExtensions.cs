using Microsoft.Extensions.DependencyInjection;
using ResourcePlanningAccelist.Commons.Security;
using ResourcePlanningAccelist.Infrastructure.Security;

namespace ResourcePlanningAccelist.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        
        return services;
    }
}
