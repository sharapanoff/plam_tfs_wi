using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TfsViewer.App.Infrastructure;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Models;

namespace TfsViewer.App.ViewModels;

/// <summary>
/// ViewModel for TFS connection settings
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ITfsService _tfsService;
    private readonly ICredentialStore _credentialStore;
    private readonly Configuration _configuration;
    private readonly Window _window;

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    private bool _useWindowsAuthentication = true;

    [ObservableProperty]
    private string? _browserExePath;

    [ObservableProperty]
    private string? _browserArgument;

    [ObservableProperty]
    private string? _vsExePath;

    [ObservableProperty]
    private string? _vsArgument;

    [ObservableProperty]
    private string? _connectionStatus;

    [ObservableProperty]
    private Brush _connectionStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isConnecting;

    public bool ConnectionSuccessful { get; private set; }

    public SettingsViewModel(ITfsService tfsService, ICredentialStore credentialStore, Configuration configuration, Window window)
    {
        _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _window = window ?? throw new ArgumentNullException(nameof(window));

        // Load saved settings
        LoadSavedSettings();
    }

    private void LoadSavedSettings()
    {
        var credentials = _credentialStore.LoadCredentials();
        if (credentials != null)
        {
            ServerUrl = credentials.ServerUrl;
            UseWindowsAuthentication = credentials.UseWindowsAuthentication;
            BrowserExePath = credentials.BrowserExePath;
            BrowserArgument = credentials.BrowserArgument;
            VsExePath = credentials.VsExePath;
            VsArgument = credentials.VsArgument;
        }
        else if (!string.IsNullOrWhiteSpace(_configuration.LastServerUrl))
        {
            ServerUrl = _configuration.LastServerUrl;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
        {
            ConnectionStatus = "Please enter a server URL";
            ConnectionStatusColor = Brushes.Red;
            return;
        }

        IsConnecting = true;
        ErrorMessage = null;
        ConnectionStatus = "Testing connection...";
        ConnectionStatusColor = Brushes.Blue;

        try
        {
            var credentials = new CConsts
            {
                ServerUrl = ServerUrl.Trim(),
                UseWindowsAuthentication = UseWindowsAuthentication,
                BrowserExePath = string.IsNullOrWhiteSpace(BrowserExePath) ? null : BrowserExePath.Trim(),
                BrowserArgument = string.IsNullOrWhiteSpace(BrowserArgument) ? null : BrowserArgument.Trim(),
                VsExePath = string.IsNullOrWhiteSpace(VsExePath) ? null : VsExePath.Trim(),
                VsArgument = string.IsNullOrWhiteSpace(VsArgument) ? null : VsArgument.Trim()
            };


            var result = await _tfsService.ConnectAsync(credentials);

            if (result.Success)
            {
                ConnectionStatus = $"✓ Connected as {result.AuthenticatedUser}";
                ConnectionStatusColor = Brushes.Green;
            }
            else
            {
                ConnectionStatus = "✗ Connection failed";
                ConnectionStatusColor = Brushes.Red;
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            ConnectionStatus = "✗ Connection failed";
            ConnectionStatusColor = Brushes.Red;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
        {
            ErrorMessage = "Server URL is required";
            return;
        }

        IsConnecting = true;
        ErrorMessage = null;

        try
        {
            var credentials = new CConsts
            {
                ServerUrl = ServerUrl.Trim(),
                UseWindowsAuthentication = UseWindowsAuthentication,
                BrowserExePath = string.IsNullOrWhiteSpace(BrowserExePath) ? null : BrowserExePath.Trim(),
                BrowserArgument = string.IsNullOrWhiteSpace(BrowserArgument) ? null : BrowserArgument.Trim(),
                VsExePath = string.IsNullOrWhiteSpace(VsExePath) ? null : VsExePath.Trim(),
                VsArgument = string.IsNullOrWhiteSpace(VsArgument) ? null : VsArgument.Trim()
            };

            var result = await _tfsService.ConnectAsync(credentials);

            if (result.Success)
            {
                // Save credentials and configuration
                _credentialStore.SaveCredentials(credentials);
                _configuration.LastServerUrl = ServerUrl.Trim();
                _configuration.Save();

                ConnectionSuccessful = true;
                _window.DialogResult = true;
                _window.Close();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Connection failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsConnecting = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        ConnectionSuccessful = false;
        _window.DialogResult = false;
        _window.Close();
    }
}
