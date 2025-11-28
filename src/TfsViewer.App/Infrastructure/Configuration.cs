using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TfsViewer.App.Contracts;

namespace TfsViewer.App.Infrastructure;

/// <summary>
/// Application configuration settings - implements IAppConfiguration interface
/// </summary>
public class Configuration : IAppConfiguration
{
    private static readonly string AppDataFolder = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TfsViewer");
    
    private static readonly string ConfigFile = Path.Combine(AppDataFolder, "appsettings.json");

    public string? LastServerUrl { get; set; }
    
    public int RefreshIntervalMinutes { get; set; } = 5;
    
    public bool AutoRefreshEnabled { get; set; } = true;
    
    public string? LastProject { get; set; }

    public bool UseWindowsAuthentication { get; set; } = true;

    public string? BrowserExePath { get; set; }

    public string? BrowserArgument { get; set; }

    public string? VsExePath { get; set; }

    public string? VsArgument { get; set; }

    public void Load()
    {
        if (!File.Exists(ConfigFile))
            return;

        try
        {
            var json = File.ReadAllText(ConfigFile);
            var loaded = JsonSerializer.Deserialize<Configuration>(json);
            if (loaded != null)
            {
                LastServerUrl = loaded.LastServerUrl;
                RefreshIntervalMinutes = loaded.RefreshIntervalMinutes;
                AutoRefreshEnabled = loaded.AutoRefreshEnabled;
                LastProject = loaded.LastProject;
                UseWindowsAuthentication = loaded.UseWindowsAuthentication;
                BrowserExePath = loaded.BrowserExePath;
                BrowserArgument = loaded.BrowserArgument;
                VsExePath = loaded.VsExePath;
                VsArgument = loaded.VsArgument;
            }
        }
        catch
        {
            // Keep default values
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(AppDataFolder);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        File.WriteAllText(ConfigFile, json);
    }
}
