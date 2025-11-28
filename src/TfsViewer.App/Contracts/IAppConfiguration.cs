using TfsViewer.Core.Contracts;

namespace TfsViewer.App.Contracts;

/// <summary>
/// Interface for application-specific configuration settings
/// </summary>
public interface IAppConfiguration:
    ITfsConfiguration, 
    IBrowserConfiguration, 
    IVsConfiguration

{
    /// <summary>
    /// Gets or sets the last used server URL
    /// </summary>
    string? LastServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the refresh interval in minutes
    /// </summary>
    int RefreshIntervalMinutes { get; set; }

    /// <summary>
    /// Gets or sets whether auto-refresh is enabled
    /// </summary>
    bool AutoRefreshEnabled { get; set; }

    /// <summary>
    /// Gets or sets the last used project
    /// </summary>
    string? LastProject { get; set; }

    public bool isValid { get; set; }
}