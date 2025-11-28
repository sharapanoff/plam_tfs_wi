namespace TfsViewer.App.Contracts;

/// <summary>
/// Interface for application configuration used in TfsViewer.App
/// </summary>
public interface IAppConfiguration
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

    /// <summary>
    /// Gets or sets whether to use Windows authentication
    /// </summary>
    bool UseWindowsAuthentication { get; set; }

    /// <summary>
    /// Gets or sets the custom browser executable path (optional)
    /// </summary>
    string? BrowserExePath { get; set; }

    /// <summary>
    /// Gets or sets the browser argument (optional)
    /// </summary>
    string? BrowserArgument { get; set; }

    /// <summary>
    /// Gets or sets the Visual Studio executable path (optional)
    /// </summary>
    string? VsExePath { get; set; }

    /// <summary>
    /// Gets or sets the Visual Studio argument (optional)
    /// </summary>
    string? VsArgument { get; set; }

    /// <summary>
    /// Loads configuration from storage
    /// </summary>
    void Load();

    /// <summary>
    /// Saves configuration to storage
    /// </summary>
    void Save();
}