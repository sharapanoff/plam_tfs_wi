namespace TfsViewer.Core.Contracts;

/// <summary>
/// Interface for browser configuration settings
/// </summary>
public interface IBrowserConfiguration
{
    /// <summary>
    /// Custom browser executable path for opening URLs (optional)
    /// </summary>
    string? BrowserExePath { get; set; }

    /// <summary>
    /// Argument to pass to custom browser before the URL (optional)
    /// </summary>
    string? BrowserArgument { get; set; }
}