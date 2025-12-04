namespace TfsViewer.App.Services;

/// <summary>
/// Service interface for launching TFS items in browser or Visual Studio
/// </summary>
public interface ILauncherService
{
    void OpenInBrowser(string url);
    
    void OpenWorkItemInVisualStudio(int workItemId, string serverUrl);
    
    void OpenWorkItemInVisualStudio(object? workItemViewModel);
    
    void OpenPullRequestInVisualStudio(int pullRequestId, string repositoryUrl);
    
    void OpenPullRequestInVisualStudio(object? pullRequestViewModel);
    
    void OpenCodeReviewInVisualStudio(int codeReviewId, string serverUrl);
    
    void OpenCodeReviewInVisualStudio(object? codeReviewViewModel);
}
