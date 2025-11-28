using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace TfsViewer.Services;

public class LauncherService : ILauncherService
{
    public bool IsVisualStudioInstalled
    {
        get
        {
            try
            {
                // Check for Visual Studio installation via registry
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\Setup");
                if (key != null)
                {
                    var versions = key.GetSubKeyNames();
                    return versions.Any(v => !string.IsNullOrEmpty(v));
                }

                // Fallback: check for devenv.exe in common paths
                var commonPaths = new[]
                {
                    @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
                    @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
                    @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
                    @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.exe",
                    @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe",
                    @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"
                };

                return commonPaths.Any(File.Exists);
            }
            catch
            {
                return false;
            }
        }
    }

    public async Task LaunchVisualStudioAsync(string url)
    {
        if (!IsVisualStudioInstalled)
            throw new InvalidOperationException("Visual Studio is not installed.");

        try
        {
            // Launch Visual Studio with the URL
            var startInfo = new ProcessStartInfo
            {
                FileName = "devenv.exe",
                Arguments = $"/edit \"{url}\"",
                UseShellExecute = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await Task.CompletedTask; // Process started asynchronously
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to launch Visual Studio: {ex.Message}", ex);
        }
    }
}