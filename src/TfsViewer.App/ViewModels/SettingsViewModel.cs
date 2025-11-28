using System.Windows;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TfsViewer.App.Contracts;
using TfsViewer.App.Infrastructure;
using TfsViewer.App.Models;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Services;

namespace TfsViewer.App.ViewModels;

/// <summary>
/// ViewModel for TFS connection settings
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ITfsService tfsService;
    private readonly IConfigStore configStore;

    private readonly AppConfiguration configuration;
    private readonly Window _window;

    #region ObservableProperty
    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    private string? _serverUrlError;

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
    #endregion

    #region RelayCommand
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

			var result = await this.tfsService.ConnectAsync(GetTfsConfiguration());

			if(result.Success)
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
            var result = await this.tfsService.ConnectAsync(GetTfsConfiguration());

            if (result.Success)
            {
                SavePropertiesToConfiguration();
		        this.configStore.Save(this.configuration);

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
    #endregion

    public bool ConnectionSuccessful { get; private set; }

    public SettingsViewModel(
        ITfsService tfsService, 
        IConfigStore configStore, 
        AppConfiguration configuration,
        Window window)
    {
        this.tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        this.configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));

        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        _window = window ?? throw new ArgumentNullException(nameof(window));

        LoadPropertiesFromConfiguration();
    }

    private void LoadPropertiesFromConfiguration()
    {
        ServerUrl = configuration.ServerUrl ?? string.Empty;//todo
        UseWindowsAuthentication = configuration.UseWindowsAuthentication;
        BrowserExePath = configuration.BrowserExePath;
        BrowserArgument = configuration.BrowserArgument;
        VsExePath = configuration.VsExePath;
        VsArgument = configuration.VsArgument;
    }

    private void SavePropertiesToConfiguration()
    {
        configuration.ServerUrl = ServerUrl;
        configuration.UseWindowsAuthentication = UseWindowsAuthentication;
        configuration.BrowserExePath = BrowserExePath;
        configuration.BrowserArgument = BrowserArgument;
        configuration.VsExePath = VsExePath;
        configuration.VsArgument = VsArgument;
    }

	private AppConfiguration GetTfsConfiguration()
	{
		return new AppConfiguration
		{
			ServerUrl = ServerUrl.Trim(),
			UseWindowsAuthentication = UseWindowsAuthentication,
			BrowserExePath = string.IsNullOrWhiteSpace(BrowserExePath) ? null : BrowserExePath.Trim(),
			BrowserArgument = string.IsNullOrWhiteSpace(BrowserArgument) ? null : BrowserArgument.Trim(),
			VsExePath = string.IsNullOrWhiteSpace(VsExePath) ? null : VsExePath.Trim(),
			VsArgument = string.IsNullOrWhiteSpace(VsArgument) ? null : VsArgument.Trim()
		};
	}


    private string? ValidateServerUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "Server URL is required";
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return "Invalid URL format";
        }

        if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
        {
            return "URL must start with http:// or https://";
        }

        return null; // Valid
    }


}
