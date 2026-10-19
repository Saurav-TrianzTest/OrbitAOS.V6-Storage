// ============================================================
// HomeService.cs – Infrastructure Layer
// Implements IHomeService from OrbitAOS.Application.
//
// In Clean Architecture, application service implementations
// that require infrastructure dependencies (DbContext, external
// APIs) live in the Infrastructure layer.
// ============================================================

using Microsoft.Extensions.Logging;
using OrbitAOS.Application.Interfaces;

namespace OrbitAOS.Infrastructure.Services
{
    /// <summary>
    /// Infrastructure implementation of <see cref="IHomeService"/>.
    /// Orchestrates data retrieval for the Home feature.
    /// </summary>
    public class HomeService : IHomeService
    {
        private readonly ILogger<HomeService> _logger;

        public HomeService(ILogger<HomeService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<HomeDashboardDto> GetDashboardDataAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching dashboard data at {Time}", DateTime.UtcNow);

            // TODO: Replace with real data retrieval from repositories
            await Task.CompletedTask;

            return new HomeDashboardDto
            {
                ApplicationName = "OrbitAOS",
                LastRefreshed = DateTime.UtcNow,
                WelcomeMessage = "Welcome to OrbitAOS on .NET 10"
            };
        }
    }
}
