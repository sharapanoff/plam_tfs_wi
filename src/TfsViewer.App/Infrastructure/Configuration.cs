using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TfsViewer.App.Infrastructure;

/// <summary>
/// Application configuration settings
/// </summary>
public class Configuration
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

    public static Configuration Load()
    {
        if (!File.Exists(ConfigFile))
            return new Configuration();

        try
        {
            var json = File.ReadAllText(ConfigFile);
            return JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();
        }
        catch
        {
            return new Configuration();
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
