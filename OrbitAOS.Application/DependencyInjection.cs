// ============================================================
// DependencyInjection.cs – Application Layer
// Extension method to register all Application-layer services
// with the ASP.NET Core DI container.
//
// Called from Program.cs:
//   builder.Services.AddApplicationServices();
// ============================================================

using Microsoft.Extensions.DependencyInjection;

namespace OrbitAOS.Application
{
    /// <summary>
    /// Extension methods for registering Application-layer services.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers all Application-layer services with the DI container.
        /// Add new service registrations here as the application grows.
        /// </summary>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            // Register application services here.
            // Example:
            //   services.AddScoped<IHomeService, HomeService>();
            //
            // Note: IHomeService implementation (HomeService) lives in
            // OrbitAOS.Infrastructure or OrbitAOS.Application.Services.
            // Register it in OrbitAOS.Infrastructure.DependencyInjection
            // to keep the Application layer free of infrastructure concerns.

            return services;
        }
    }
}
