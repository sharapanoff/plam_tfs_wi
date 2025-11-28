using TfsViewer.Core.Models;

namespace TfsViewer.Core.Contracts;

/// <summary>
/// Interface for core TFS configuration used in TfsViewer.Core
/// </summary>
public interface ICoreConfiguration
{
    /// <summary>
    /// Gets or sets the TFS server URL
    /// </summary>
    string ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets whether to use Windows authentication
    /// </summary>
    bool UseWindowsAuthentication { get; set; }

    /// <summary>
    /// Gets or sets the personal access token (optional)
    /// </summary>
    string? PersonalAccessToken { get; set; }

    /// <summary>
    /// Gets or sets the domain for domain authentication (optional)
    /// </summary>
    string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the username for domain authentication (optional)
    /// </summary>
    string? Username { get; set; }

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
}