using System.IO;
using System.Text.Json;
using TfsViewer.App.Contracts;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Models;

namespace TfsViewer.App.Infrastructure;

/// <summary>
/// Unified application configuration that implements both core and app interfaces
/// Stores all configuration in a single appsettings.json file
/// </summary>
public class AppConfiguration : ICoreConfiguration, IAppConfiguration
{
    private static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TfsViewer");

    private static readonly string ConfigFile = Path.Combine(AppDataFolder, "appsettings.json");

    // Core configuration properties (from ICoreConfiguration)
    public string ServerUrl { get; set; } = string.Empty;
    public bool UseWindowsAuthentication { get; set; } = true;
    public string? PersonalAccessToken { get; set; }
    public string? Domain { get; set; }
    public string? Username { get; set; }
    public string? BrowserExePath { get; set; }
    public string? BrowserArgument { get; set; }
    public string? VsExePath { get; set; }
    public string? VsArgument { get; set; }

    // App configuration properties (from IAppConfiguration)
    public string? LastServerUrl { get; set; }
    public int RefreshIntervalMinutes { get; set; } = 5;
    public bool AutoRefreshEnabled { get; set; } = true;
    public string? LastProject { get; set; }

    /// <summary>
    /// Loads configuration from appsettings.json
    /// </summary>
    public void Load()
    {
        if (!File.Exists(ConfigFile))
            return;

        try
        {
            var json = File.ReadAllText(ConfigFile);
            var loaded = JsonSerializer.Deserialize<AppConfiguration>(json);
            if (loaded != null)
            {
                // Copy all properties from loaded configuration
                ServerUrl = loaded.ServerUrl;
                UseWindowsAuthentication = loaded.UseWindowsAuthentication;
                PersonalAccessToken = loaded.PersonalAccessToken;
                Domain = loaded.Domain;
                Username = loaded.Username;
                BrowserExePath = loaded.BrowserExePath;
                BrowserArgument = loaded.BrowserArgument;
                VsExePath = loaded.VsExePath;
                VsArgument = loaded.VsArgument;
                LastServerUrl = loaded.LastServerUrl;
                RefreshIntervalMinutes = loaded.RefreshIntervalMinutes;
                AutoRefreshEnabled = loaded.AutoRefreshEnabled;
                LastProject = loaded.LastProject;
            }
        }
        catch
        {
            // If loading fails, keep default values
        }
    }

    /// <summary>
    /// Saves configuration to appsettings.json
    /// </summary>
    public void Save()
    {
        Directory.CreateDirectory(AppDataFolder);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(ConfigFile, json);
    }

    /// <summary>
    /// Creates a CConsts object from the core configuration properties
    /// </summary>
    public CConsts ToCredentials()
    {
        return new CConsts
        {
            ServerUrl = ServerUrl,
            UseWindowsAuthentication = UseWindowsAuthentication,
            PersonalAccessToken = PersonalAccessToken,
            Domain = Domain,
            Username = Username,
            BrowserExePath = BrowserExePath,
            BrowserArgument = BrowserArgument,
            VsExePath = VsExePath,
            VsArgument = VsArgument
        };
    }

    /// <summary>
    /// Updates configuration from a CConsts object
    /// </summary>
    public void FromCredentials(CConsts credentials)
    {
        if (credentials == null) return;

        ServerUrl = credentials.ServerUrl;
        UseWindowsAuthentication = credentials.UseWindowsAuthentication;
        PersonalAccessToken = credentials.PersonalAccessToken;
        Domain = credentials.Domain;
        Username = credentials.Username;
        BrowserExePath = credentials.BrowserExePath;
        BrowserArgument = credentials.BrowserArgument;
        VsExePath = credentials.VsExePath;
        VsArgument = credentials.VsArgument;
    }
}