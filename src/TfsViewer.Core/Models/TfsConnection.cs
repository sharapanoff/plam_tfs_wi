namespace TfsViewer.Core.Models;

/// <summary>
/// Represents connection information for TFS server
/// </summary>
public class TfsConnection
{
    public string ServerUrl { get; set; } = string.Empty;
    
    public string CollectionName { get; set; } = string.Empty;
    
    public string ProjectName { get; set; } = string.Empty;
    
    public bool UseWindowsAuthentication { get; set; } = true;
    
    public bool IsConnected { get; set; }
    
    public DateTime? LastConnected { get; set; }
}
