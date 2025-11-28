namespace TfsViewer.Core.Contracts;

/// <summary>
/// Represents TFS authentication credentials
/// </summary>
public interface ITfsConfiguration
{
    public string ServerUrl { get; set; }
    
    public bool UseWindowsAuthentication { get; set; }
    
    public string? PersonalAccessToken { get; set; }
    
    public string? Domain { get; set; }
    
    public string? Username { get; set; }
}
