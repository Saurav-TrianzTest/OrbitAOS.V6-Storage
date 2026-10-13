namespace OrbitAOS.V6.Web.Models
{
    /// <summary>
    /// ViewModel for displaying error information.
    /// Migrated from legacy ASP.NET MVC HandleErrorAttribute pattern.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
