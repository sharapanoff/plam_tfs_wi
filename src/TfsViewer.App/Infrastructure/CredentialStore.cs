using System.Text.Json;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Models;

namespace TfsViewer.App.Infrastructure;

/// <summary>
/// Stores credentials using the unified configuration system
/// </summary>
public class CredentialStore : ICredentialStore
{
    private readonly ICoreConfiguration _configuration;

    public CredentialStore(ICoreConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void SaveCredentials(CConsts credentials)
    {
        if (credentials == null)
            throw new ArgumentNullException(nameof(credentials));

        // Update the configuration with credential data
        _configuration.ServerUrl = credentials.ServerUrl;
        _configuration.UseWindowsAuthentication = credentials.UseWindowsAuthentication;
        _configuration.PersonalAccessToken = credentials.PersonalAccessToken;
        _configuration.Domain = credentials.Domain;
        _configuration.Username = credentials.Username;
        _configuration.BrowserExePath = credentials.BrowserExePath;
        _configuration.BrowserArgument = credentials.BrowserArgument;
        _configuration.VsExePath = credentials.VsExePath;
        _configuration.VsArgument = credentials.VsArgument;

        // Note: Saving is handled by the app layer that owns the configuration
    }

    public CConsts? LoadCredentials()
    {
        // Return credentials from configuration
        return new CConsts
        {
            ServerUrl = _configuration.ServerUrl,
            UseWindowsAuthentication = _configuration.UseWindowsAuthentication,
            PersonalAccessToken = _configuration.PersonalAccessToken,
            Domain = _configuration.Domain,
            Username = _configuration.Username,
            BrowserExePath = _configuration.BrowserExePath,
            BrowserArgument = _configuration.BrowserArgument,
            VsExePath = _configuration.VsExePath,
            VsArgument = _configuration.VsArgument
        };
    }

    public void ClearCredentials()
    {
        // Clear credential fields in configuration
        _configuration.ServerUrl = string.Empty;
        _configuration.UseWindowsAuthentication = true;
        _configuration.PersonalAccessToken = null;
        _configuration.Domain = null;
        _configuration.Username = null;
        _configuration.BrowserExePath = null;
        _configuration.BrowserArgument = null;
        _configuration.VsExePath = null;
        _configuration.VsArgument = null;

        // Note: Saving is handled by the app layer that owns the configuration
    }

    public bool HasStoredCredentials()
    {
        return !string.IsNullOrEmpty(_configuration.ServerUrl);
    }
}