using TfsViewer.Core.Contracts;

namespace TfsViewer.Core.Models;

/// <summary>
/// Represents TFS authentication credentials
/// </summary>
public class CConsts : IConstsTFS
{
    public string ServerUrl { get; set; } = string.Empty;
    
    public bool UseWindowsAuthentication { get; set; } = true;
    
    public string? PersonalAccessToken { get; set; }
    
    public string? Domain { get; set; }
    
    public string? Username { get; set; }
    
    /// <summary>
    /// Custom browser executable path for opening URLs (optional)
    /// </summary>
    public string? BrowserExePath { get; set; }
    
    /// <summary>
    /// Argument to pass to custom browser before the URL (optional)
    /// </summary>
    public string? BrowserArgument { get; set; }

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
}
