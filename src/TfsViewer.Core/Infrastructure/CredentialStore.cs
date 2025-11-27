using System.Text.Json;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Models;

namespace TfsViewer.Core.Infrastructure;

/// <summary>
/// Stores credentials in user's local application data folder
/// </summary>
public class CredentialStore : ICredentialStore
{
    private static readonly string AppDataFolder = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TfsViewer");
    
    private static readonly string CredentialsFile = Path.Combine(AppDataFolder, "credentials.json");

    public void SaveCredentials(CConsts credentials)
    {
        if (credentials == null)
            throw new ArgumentNullException(nameof(credentials));

        Directory.CreateDirectory(AppDataFolder);

        var json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        File.WriteAllText(CredentialsFile, json);
    }

    public CConsts? LoadCredentials()
    {
        if (!File.Exists(CredentialsFile))
            return null;

        try
        {
            var json = File.ReadAllText(CredentialsFile);
            return JsonSerializer.Deserialize<CConsts>(json);
        }
        catch
        {
            return null;
        }
    }

    public void ClearCredentials()
    {
        if (File.Exists(CredentialsFile))
        {
            File.Delete(CredentialsFile);
        }
    }

    public bool HasStoredCredentials()
    {
        return File.Exists(CredentialsFile);
    }
}
