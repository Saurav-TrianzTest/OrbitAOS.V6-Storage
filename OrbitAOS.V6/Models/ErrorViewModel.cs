namespace OrbitAOS.V6.Models
{
    /// <summary>
    /// View model for the Error view.
    /// RequestId is populated from the distributed trace (Activity.Current?.Id)
    /// or the ASP.NET Core request identifier (HttpContext.TraceIdentifier).
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// The unique identifier for the failed request.
        /// Nullable because it may not be available in all hosting environments.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Returns true when RequestId has a non-empty value,
        /// controlling whether the Request ID block is rendered in the Error view.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
