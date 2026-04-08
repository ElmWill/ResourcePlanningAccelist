using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ResourcePlanningAccelist.Commons.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCommons(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}