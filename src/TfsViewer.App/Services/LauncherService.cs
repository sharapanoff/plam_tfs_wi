using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace TfsViewer.App.Services;

/// <summary>
/// Service for launching TFS items in browser or Visual Studio
/// </summary>
public class LauncherService : ILauncherService
{
    private readonly TfsViewer.Core.Contracts.ICredentialStore _credentialStore;

    public LauncherService(TfsViewer.Core.Contracts.ICredentialStore credentialStore)
    {
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
    }

    public void OpenInBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            var credentials = _credentialStore.LoadCredentials();
            var browserExePath = credentials?.BrowserExePath;
            var browserArgument = credentials?.BrowserArgument;

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
            var credentials = _credentialStore.LoadCredentials();
            var vsExePath = credentials?.VsExePath;
            var vsArgument = credentials?.VsArgument;

            if (string.IsNullOrWhiteSpace(vsExePath) || !File.Exists(vsExePath))
            {
                throw new InvalidOperationException("Visual Studio path is not configured or invalid");
            }

            var workItemUrl = $"{serverUrl}/_workitems/edit/{workItemId}";
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
            var credentials = _credentialStore.LoadCredentials();
            var vsExePath = credentials?.VsExePath;
            var vsArgument = credentials?.VsArgument;

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
            var credentials = _credentialStore.LoadCredentials();
            var vsExePath = credentials?.VsExePath;
            var vsArgument = credentials?.VsArgument;

            if (string.IsNullOrWhiteSpace(vsExePath) || !File.Exists(vsExePath))
            {
                throw new InvalidOperationException("Visual Studio path is not configured or invalid");
            }

            var reviewUrl = $"{serverUrl}/_workitems/edit/{codeReviewId}";
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
