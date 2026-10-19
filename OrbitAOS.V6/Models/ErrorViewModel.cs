// ============================================================
// ErrorViewModel.cs – Models layer (Web presentation models)
// .NET 10 compatible – no changes required from .NET 6 baseline.
// Nullable reference types are enabled via project settings.
// ============================================================

namespace OrbitAOS.V6.Models
{
    /// <summary>
    /// View model for the shared Error view.
    /// Carries the ASP.NET Core request trace identifier for diagnostics.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// The current request ID from Activity.Current or HttpContext.TraceIdentifier.
        /// Nullable because it may not be available in all error scenarios.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Returns true when a non-empty RequestId is available to display.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
