using System.Diagnostics;
using System.IO;

using Microsoft.Win32;

using TfsViewer.App.Contracts;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Models;

namespace TfsViewer.App.Services;

/// <summary>
/// Service for launching TFS items in browser or Visual Studio
/// </summary>
public class LauncherService : ILauncherService
{
    private readonly IBrowserConfiguration browserConfiguration;
    private readonly IVsConfiguration visualStudioConfiguration;
    private readonly ITfsConfiguration tfsConfiguration;

    public LauncherService(
        IBrowserConfiguration browserConfiguration,
        IVsConfiguration visualStudioConfiguration,
        ITfsConfiguration tfsConfiguration 
        )
    {
        this.browserConfiguration = browserConfiguration ?? throw new ArgumentNullException(nameof(browserConfiguration));
        this.visualStudioConfiguration = visualStudioConfiguration ?? throw new ArgumentNullException(nameof(visualStudioConfiguration));
        this.tfsConfiguration = tfsConfiguration ?? throw new ArgumentNullException(nameof(tfsConfiguration));
    }

    public void OpenInBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            var browserExePath = browserConfiguration?.BrowserExePath;
            var browserArgument = browserConfiguration?.BrowserArgument;

            if (!string.IsNullOrWhiteSpace(browserExePath) && File.Exists(browserExePath))
            {
                // Use custom browser with optional argument
                var arguments = string.IsNullOrWhiteSpace(browserArgument) 
                    ? url 
                    : $"{browserArgument} \"{url}\"";
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = browserExePath,
                    Arguments = arguments,
                    UseShellExecute = false
                });
            }
            else
            {
                // Use default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open URL in browser: {ex.Message}", ex);
        }
    }

    public void OpenWorkItemInVisualStudio(int workItemId, string serverUrl)
    {
        try
        {
            var vsExePath = visualStudioConfiguration?.VsExePath;
            var vsArgument = visualStudioConfiguration?.VsArgument;

            if (string.IsNullOrWhiteSpace(vsExePath) || !File.Exists(vsExePath))
            {
                throw new InvalidOperationException("Visual Studio path is not configured or invalid");
            }

            var workItemUrl = 
                "";
                //$" /TfsLink \"vstfs:///WorkItemTracking/WorkItem/{workItemId}?url={visualStudioConfiguration?.ServerUrl}\"";
                //$"{serverUrl}/_workitems/edit/{workItemId}";
            var arguments = string.IsNullOrWhiteSpace(vsArgument) ? workItemUrl : $"{vsArgument} \"{workItemUrl}\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = vsExePath,
                Arguments = arguments,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open work item in Visual Studio: {ex.Message}", ex);
        }
    }

    public void OpenPullRequestInVisualStudio(int pullRequestId, string repositoryUrl)
    {
        try
        {
            var vsExePath = visualStudioConfiguration?.VsExePath;
            var vsArgument = visualStudioConfiguration?.VsArgument;

            if (string.IsNullOrWhiteSpace(vsExePath) || !File.Exists(vsExePath))
            {
                throw new InvalidOperationException("Visual Studio path is not configured or invalid");
            }

            var prUrl = $"{repositoryUrl}/pullrequest/{pullRequestId}";
            var arguments = string.IsNullOrWhiteSpace(vsArgument) ? prUrl : $"{vsArgument} \"{prUrl}\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = vsExePath,
                Arguments = arguments,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open pull request: {ex.Message}", ex);
        }
    }

    public void OpenCodeReviewInVisualStudio(int codeReviewId, string serverUrl)
    {
        try
        {
            var vsExePath = visualStudioConfiguration?.VsExePath;
            var vsArgument = visualStudioConfiguration?.VsArgument;

            if (string.IsNullOrWhiteSpace(vsExePath) || !File.Exists(vsExePath))
            {
                throw new InvalidOperationException("Visual Studio path is not configured or invalid");
            }

            var reviewUrl = 
                $" /TfsLink \"vstfs:///CodeReview/CodeReviewID/{codeReviewId}?url={tfsConfiguration?.ServerUrl}\"";
                //$"vstfs:///CodeReview/ReviewId/{codeReviewId}";
                // $"{serverUrl}/_workitems/edit/{codeReviewId}";
            var arguments = string.IsNullOrWhiteSpace(vsArgument) ? reviewUrl : $"{vsArgument} \"{reviewUrl}\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = vsExePath,
                Arguments = arguments,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open code review: {ex.Message}", ex);
        }
    }

    // VS detection removed; VS launching relies on user-configured path/argument
}
