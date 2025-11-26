namespace TfsViewer.Core.Models;

/// <summary>
/// Represents a TFS/Azure DevOps pull request
/// </summary>
public class PullRequest
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Repository { get; set; } = string.Empty;
    
    public string SourceBranch { get; set; } = string.Empty;
    
    public string TargetBranch { get; set; } = string.Empty;
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;
    
    public bool IsDraft { get; set; }
}
