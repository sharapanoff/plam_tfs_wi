namespace TfsViewer.Core.Models;

/// <summary>
/// Represents TFS authentication credentials
/// </summary>
public class CConsts
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
}
