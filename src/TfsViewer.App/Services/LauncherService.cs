using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace TfsViewer.App.Services;

/// <summary>
/// Service for launching TFS items in browser or Visual Studio
/// </summary>
public class LauncherService : ILauncherService
{
    private string? _cachedVsPath;

    public void OpenInBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open URL in browser: {ex.Message}", ex);
        }
    }

    public void OpenWorkItemInVisualStudio(int workItemId, string serverUrl)
    {
        if (!IsVisualStudioInstalled())
        {
            throw new InvalidOperationException("Visual Studio is not installed");
        }

        try
        {
            var vsPath = GetVisualStudioPath();
            if (string.IsNullOrWhiteSpace(vsPath))
            {
                throw new InvalidOperationException("Could not locate Visual Studio");
            }

            // Use vs:// protocol handler to open work item
            var vsUri = $"vs://workitem/{workItemId}";
            Process.Start(new ProcessStartInfo
            {
                FileName = vsUri,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open work item in Visual Studio: {ex.Message}", ex);
        }
    }

    public void OpenPullRequestInVisualStudio(int pullRequestId, string repositoryUrl)
    {
        if (!IsVisualStudioInstalled())
        {
            throw new InvalidOperationException("Visual Studio is not installed");
        }

        try
        {
            // Use browser as fallback for PR (VS support varies)
            var prUrl = $"{repositoryUrl}/pullrequest/{pullRequestId}";
            OpenInBrowser(prUrl);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open pull request: {ex.Message}", ex);
        }
    }

    public void OpenCodeReviewInVisualStudio(int codeReviewId, string serverUrl)
    {
        if (!IsVisualStudioInstalled())
        {
            throw new InvalidOperationException("Visual Studio is not installed");
        }

        try
        {
            // Use browser as fallback for code reviews
            var reviewUrl = $"{serverUrl}/_workitems/edit/{codeReviewId}";
            OpenInBrowser(reviewUrl);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open code review: {ex.Message}", ex);
        }
    }

    public bool IsVisualStudioInstalled()
    {
        var vsPath = GetVisualStudioPath();
        return !string.IsNullOrWhiteSpace(vsPath) && File.Exists(vsPath);
    }

    public string? GetVisualStudioPath()
    {
        if (_cachedVsPath != null)
            return _cachedVsPath;

        // Try vswhere utility first (recommended method)
        var vsWherePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio", "Installer", "vswhere.exe");

        if (File.Exists(vsWherePath))
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = vsWherePath,
                    Arguments = "-latest -products * -requires Microsoft.Component.MSBuild -property installationPath",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var installPath = output.Trim();
                        var devEnvPath = Path.Combine(installPath, "Common7", "IDE", "devenv.exe");

                        if (File.Exists(devEnvPath))
                        {
                            _cachedVsPath = devEnvPath;
                            return _cachedVsPath;
                        }
                    }
                }
            }
            catch
            {
                // Fall through to registry check
            }
        }

        // Fallback: Check registry for VS installation (VS 2022 = 17.0 per FR-029)
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\SxS\VS7");
            if (key != null)
            {
                // Try VS 2022 (17.0) first per FR-029, then fallback to older versions
                foreach (var version in new[] { "17.0", "16.0", "15.0" })
                {
                    var installPath = key.GetValue(version) as string;
                    if (!string.IsNullOrWhiteSpace(installPath))
                    {
                        var devEnvPath = Path.Combine(installPath, "Common7", "IDE", "devenv.exe");
                        if (File.Exists(devEnvPath))
                        {
                            _cachedVsPath = devEnvPath;
                            return _cachedVsPath;
                        }
                    }
                }
            }
        }
        catch
        {
            // VS not found in registry
        }

        return null;
    }
}
