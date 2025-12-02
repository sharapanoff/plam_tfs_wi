namespace TfsViewer.Core.Models;

/// <summary>
/// Represents a TFS code review
/// </summary>
public class CodeReview
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string RequestedBy { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public string Url { get; set; } = string.Empty;
    
    public string Context { get; set; } = string.Empty;
    
    public string ProjectName { get; set; } = string.Empty;
    
    public string AreaPath { get; set; } = string.Empty;
}
