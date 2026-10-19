// ============================================================
// HomeDashboardDto.cs – Application Layer
// Data Transfer Object returned by IHomeService.GetDashboardDataAsync.
// DTOs cross the boundary between Application and Web layers.
// ============================================================

namespace OrbitAOS.Application.Interfaces
{
    /// <summary>
    /// DTO carrying dashboard summary data for the Home view.
    /// Extend with additional properties as the application grows.
    /// </summary>
    public sealed class HomeDashboardDto
    {
        /// <summary>Application display name shown in the header.</summary>
        public string ApplicationName { get; init; } = "OrbitAOS";

        /// <summary>UTC timestamp of the last data refresh.</summary>
        public DateTime LastRefreshed { get; init; } = DateTime.UtcNow;

        /// <summary>Optional welcome message for the authenticated user.</summary>
        public string? WelcomeMessage { get; init; }
    }
}
