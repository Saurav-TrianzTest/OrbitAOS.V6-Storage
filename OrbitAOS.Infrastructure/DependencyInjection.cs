// ============================================================
// DependencyInjection.cs – Infrastructure Layer
// Extension method to register all Infrastructure-layer services.
//
// Called from Program.cs:
//   builder.Services.AddInfrastructureServices(builder.Configuration);
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrbitAOS.Application.Interfaces;
using OrbitAOS.Domain.Interfaces;
using OrbitAOS.Infrastructure.Data;
using OrbitAOS.Infrastructure.Repositories;
using OrbitAOS.Infrastructure.Services;

namespace OrbitAOS.Infrastructure
{
    /// <summary>
    /// Extension methods for registering Infrastructure-layer services.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers all Infrastructure-layer services with the DI container.
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // --------------------------------------------------
            // EF Core DbContext
            // Connection string from appsettings.json
            // --------------------------------------------------
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)));

            // --------------------------------------------------
            // Generic repository registration
            // --------------------------------------------------
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // --------------------------------------------------
            // Application service implementations
            // --------------------------------------------------
            services.AddScoped<IHomeService, HomeService>();

            return services;
        }
    }
}
