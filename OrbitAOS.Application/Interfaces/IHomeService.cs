// ============================================================
// IHomeService.cs – Application Layer
// Service interface for the Home feature.
// Defined in Application; implemented in Infrastructure or Web.
// ============================================================

namespace OrbitAOS.Application.Interfaces
{
    /// <summary>
    /// Application service contract for the Home feature.
    /// Add methods here as the application grows.
    /// </summary>
    public interface IHomeService
    {
        /// <summary>
        /// Returns dashboard summary data for the home page.
        /// Async by default – all application service methods should be async.
        /// </summary>
        Task<HomeDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    }
}
