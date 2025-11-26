namespace TfsViewer.Core.Models;

/// <summary>
/// Represents a TFS work item
/// </summary>
public class WorkItem
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string WorkItemType { get; set; } = string.Empty;
    
    public string State { get; set; } = string.Empty;
    
    public DateTime? AssignedDate { get; set; }
    
    public string AssignedTo { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty;
    
    public string AreaPath { get; set; } = string.Empty;
    
    public string IterationPath { get; set; } = string.Empty;
    
    public DateTime? CreatedDate { get; set; }
    
    public DateTime? ChangedDate { get; set; }
    
    public string Url { get; set; } = string.Empty;
}
