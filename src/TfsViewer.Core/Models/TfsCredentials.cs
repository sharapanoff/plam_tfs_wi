namespace TfsViewer.Core.Models;

/// <summary>
/// Represents TFS authentication credentials
/// </summary>
public class TfsCredentials
{
    public string ServerUrl { get; set; } = string.Empty;
    
    public bool UseWindowsAuthentication { get; set; } = true;
    
    public string? PersonalAccessToken { get; set; }
    
    public string? Domain { get; set; }
    
    public string? Username { get; set; }
}
