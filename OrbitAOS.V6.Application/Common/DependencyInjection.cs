using Microsoft.Extensions.DependencyInjection;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Application.Services;

namespace OrbitAOS.V6.Application.Common;

/// <summary>
/// Dependency injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<ISampleService, SampleService>();

        // Add more service registrations here as needed

        return services;
    }
}
