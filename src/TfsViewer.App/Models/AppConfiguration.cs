using TfsViewer.Core.Contracts;
using TfsViewer.App.Contracts;

namespace TfsViewer.App.Models;

/// <summary>
/// Represents all application configuration properties
/// </summary>
public class AppConfiguration : 
    ITfsConfiguration, 
    IBrowserConfiguration, 
    IVsConfiguration, 
    IAppConfiguration
{
    // TFS Authentication properties (IConstsTFS)
    public string ServerUrl { get; set; } = string.Empty;

    public bool UseWindowsAuthentication { get; set; } = true;

    public string? PersonalAccessToken { get; set; }

    public string? Domain { get; set; }

    public string? Username { get; set; }

    // Browser configuration properties (IBrowserConfiguration)
    /// <summary>
    /// Custom browser executable path for opening URLs (optional)
    /// </summary>
    public string? BrowserExePath { get; set; }

    /// <summary>
    /// Argument to pass to custom browser before the URL (optional)
    /// </summary>
    public string? BrowserArgument { get; set; }

    // Visual Studio configuration properties (IVsConfiguration)
    /// <summary>
    /// Optional full path to Visual Studio executable for opening items in VS.
    /// If not set, VS launch features will be disabled.
    /// </summary>
    public string? VsExePath { get; set; }

    /// <summary>
    /// Optional argument to pass to Visual Studio before the item identifier.
    /// Example: "-edit" or solution path; the work item/URL will be appended after.
    /// </summary>
    public string? VsArgument { get; set; }

    // App settings properties (IAppSettingsConfiguration)
    /// <summary>
    /// Gets or sets the last used server URL
    /// </summary>
    public string? LastServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the refresh interval in minutes
    /// </summary>
    public int RefreshIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether auto-refresh is enabled
    /// </summary>
    public bool AutoRefreshEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the last used project
    /// </summary>
    public string? LastProject { get; set; }
	public bool isValid { get; set; }
}
