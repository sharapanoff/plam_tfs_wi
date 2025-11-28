using System.IO;
using System.Text.Json;
using TfsViewer.App.Contracts;
using TfsViewer.App.Models;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Models;

namespace TfsViewer.App.Infrastructure;

/// <summary>
/// Unified application configuration that implements both core and app interfaces
/// Stores all configuration in a single appsettings.json file
/// </summary>
public class ConfigStore : IConfigStore
{
    private static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TfsViewer");

    private static readonly string ConfigFile = Path.Combine(AppDataFolder, "appsettings.json");

	public bool HasStoredConfiguration()
	{
        return File.Exists(ConfigFile);
	}


	/// <summary>
	/// Loads configuration from appsettings.json
	/// </summary>
	public AppConfiguration Load()
    {
        if (!File.Exists(ConfigFile))
            return new AppConfiguration(){ isValid = false};

        try
        {
            var json = File.ReadAllText(ConfigFile);
            var loaded = JsonSerializer.Deserialize<AppConfiguration>(json);
            loaded!.isValid = true;
            return loaded;
        }
        catch
        {
            return new AppConfiguration(){ isValid = false};
        }
    }

    /// <summary>
    /// Saves configuration to appsettings.json
    /// </summary>
    public void Save(AppConfiguration consts)
    {
        Directory.CreateDirectory(AppDataFolder);

        var json = JsonSerializer.Serialize(consts, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(ConfigFile, json);
    }

}