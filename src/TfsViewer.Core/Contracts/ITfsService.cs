using TfsViewer.Core.Models;

namespace TfsViewer.Core.Contracts;

/// <summary>
/// Service interface for TFS operations
/// </summary>
public interface ITfsService
{
    Task<ConnectionResult> ConnectAsync(IConstsTFS credentials, CancellationToken cancellationToken = default);
    
    Task<ConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default);
    
    void Disconnect();
    
    Task<IEnumerable<WorkItem>> GetAssignedWorkItemsAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<PullRequest>> GetPullRequestsAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<CodeReview>> GetCodeReviewsAsync(CancellationToken cancellationToken = default);
    
    bool IsConnected { get; }
    
    string? CurrentUser { get; }
}
