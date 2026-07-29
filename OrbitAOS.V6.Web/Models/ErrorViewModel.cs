namespace OrbitAOS.V6.Web.Models;

/// <summary>
/// Error view model for displaying error information
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
