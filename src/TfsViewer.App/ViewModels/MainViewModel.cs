using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TfsViewer.App.Views;
using TfsViewer.App.Infrastructure;
using TfsViewer.Core.Contracts;

namespace TfsViewer.App.ViewModels;

/// <summary>
/// Main window view model
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ITfsService _tfsService;
    private readonly ICredentialStore _credentialStore;
    private readonly Configuration _configuration;
    private readonly ICacheService _cacheService;
    private readonly DispatcherTimer _autoRefreshTimer;

    [ObservableProperty]
    private WorkItemsTabViewModel _workItemsTab;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private DateTime? _lastRefreshTime;

    public MainViewModel(
        ITfsService tfsService,
        ICredentialStore credentialStore,
        Configuration configuration,
        ICacheService cacheService,
        WorkItemsTabViewModel workItemsTab)
    {
        _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _workItemsTab = workItemsTab ?? throw new ArgumentNullException(nameof(workItemsTab));

        // Setup auto-refresh timer (5 minutes)
        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };
        _autoRefreshTimer.Tick += async (s, e) => await AutoRefreshAsync();
    }

    public async Task InitializeAsync()
    {
        // Check if credentials exist
        if (!_credentialStore.HasStoredCredentials())
        {
            ShowSettingsWindow();
            return;
        }

        // Try to connect with saved credentials
        var credentials = _credentialStore.LoadCredentials();
        if (credentials != null)
        {
            var result = await _tfsService.ConnectAsync(credentials);
            
            if (result.Success)
            {
                StatusMessage = $"Connected as {result.AuthenticatedUser}";
                await LoadDataAsync();
                
                // Start auto-refresh timer if enabled
                if (_configuration.AutoRefreshEnabled)
                {
                    _autoRefreshTimer.Start();
                }
            }
            else
            {
                StatusMessage = "Connection failed";
                ShowSettingsWindow();
            }
        }
    }

    private async Task AutoRefreshAsync()
    {
        if (_tfsService.IsConnected && !IsLoading)
        {
            await LoadDataAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (!_tfsService.IsConnected)
        {
            ShowSettingsWindow();
            return;
        }

        // Clear cache to force fresh data fetch
        _cacheService.Clear();
        
        await LoadDataAsync();
    }

    private bool CanRefresh() => !IsLoading;

    [RelayCommand]
    private void OpenSettings()
    {
        ShowSettingsWindow();
    }

    private void ShowSettingsWindow()
    {
        var settingsWindow = new SettingsWindow();
        var settingsViewModel = new SettingsViewModel(
            _tfsService,
            _credentialStore,
            _configuration,
            settingsWindow);

        settingsWindow.DataContext = settingsViewModel;
        
        var result = settingsWindow.ShowDialog();

        if (result == true && settingsViewModel.ConnectionSuccessful)
        {
            StatusMessage = $"Connected to {_configuration.LastServerUrl}";
            _ = LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        RefreshCommand.NotifyCanExecuteChanged();
        StatusMessage = "Loading work items...";

        try
        {
            await WorkItemsTab.LoadWorkItemsAsync();
            LastRefreshTime = DateTime.Now;
            StatusMessage = $"Loaded {WorkItemsTab.WorkItemCount} work items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Failed to refresh data: {ex.Message}", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            RefreshCommand.NotifyCanExecuteChanged();
        }
    }
}
