using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TfsViewer.App.Contracts;
using TfsViewer.App.Models;
using TfsViewer.App.Views;
using TfsViewer.Core.Contracts;


namespace TfsViewer.App.ViewModels;

/// <summary>
/// Main window view model
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ICacheService _cacheService;
    private readonly ITfsService _tfsService;
    private readonly IConfigStore configStore;

    private readonly AppConfiguration configuration;
    
    private readonly DispatcherTimer _autoRefreshTimer;

    [ObservableProperty]
    private WorkItemsTabViewModel _workItemsTab;

    [ObservableProperty]
    private PullRequestTabViewModel? _pullRequestsTab;

    [ObservableProperty]
    private CodeReviewTabViewModel? _codeReviewsTab;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private DateTime? _lastRefreshTime;

    [ObservableProperty]
    private string _trayTooltip = "TFS Viewer"; // Dynamic tooltip for tray icon

    public MainViewModel(
        ITfsService tfsService,
        IConfigStore configStore,
        AppConfiguration configuration,
        ICacheService cacheService,
        WorkItemsTabViewModel workItemsTab,
        PullRequestTabViewModel? pullRequestsTab = null,
        CodeReviewTabViewModel? codeReviewsTab = null)
    {
        _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        this.configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _workItemsTab = workItemsTab ?? throw new ArgumentNullException(nameof(workItemsTab));
        _pullRequestsTab = pullRequestsTab;
        _codeReviewsTab = codeReviewsTab;

        // Setup auto-refresh timer (5 minutes)
        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };
        _autoRefreshTimer.Tick += async (s, e) => await AutoRefreshAsync();
    }

    public async Task InitializeAsync()
    {
        if (!configStore.HasStoredConfiguration())
        {
            ShowSettingsWindow();
            return;
        }

        if (configuration.isValid)
        {
            var result = await _tfsService.ConnectAsync(configuration);
            
            if (result.Success)
            {
                StatusMessage = $"Connected as {result.AuthenticatedUser}";
                await LoadDataAsync();

                // Start auto-refresh timer if enabled

                if(this.configuration.AutoRefreshEnabled)
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

    [RelayCommand]
    private void OpenAbout()
    {
        var aboutWindow = new Views.AboutWindow();
        aboutWindow.ShowDialog();
    }

    private void ShowSettingsWindow()
    {
        var settingsWindow = new SettingsWindow();
        var settingsViewModel = new SettingsViewModel(
            _tfsService,
            configStore,
            this.configuration,
            settingsWindow);

        settingsWindow.DataContext = settingsViewModel;
        
        var result = settingsWindow.ShowDialog();

        if (result == true && settingsViewModel.ConnectionSuccessful)
        {
            StatusMessage = $"Connected to {this.configuration.ServerUrl}";
            //_ = LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        RefreshCommand.NotifyCanExecuteChanged();
        StatusMessage = "Loading data...";

        try
        {
            // Load data in parallel
            var tasks = new List<Task>
            {
                WorkItemsTab.LoadWorkItemsAsync()
            };

            if (PullRequestsTab != null)
            {
                tasks.Add(PullRequestsTab.LoadPullRequestsAsync());
            }

            if (CodeReviewsTab != null)
            {
                tasks.Add(CodeReviewsTab.LoadCodeReviewsAsync());
            }

            await Task.WhenAll(tasks);

            LastRefreshTime = DateTime.Now;
            var prCount = PullRequestsTab?.PullRequestCount ?? 0;
            var crCount = CodeReviewsTab?.CodeReviewCount ?? 0;
            StatusMessage = $"Loaded {WorkItemsTab.WorkItemCount} work items, {prCount} PRs, {crCount} reviews";
            TrayTooltip = $"Work Items: {WorkItemsTab.WorkItemCount} | PRs: {prCount} | Reviews: {crCount}";
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

    // Tray commands
    [RelayCommand]
    private void ShowWindow()
    {
        var win = Application.Current?.MainWindow;
        if (win != null)
        {
            win.Show();
            win.WindowState = WindowState.Normal;
            win.Activate();
        }
    }

    [RelayCommand]
    private void HideWindow()
    {
        var win = Application.Current?.MainWindow;
        win?.Hide();
    }

    [RelayCommand]
    private void Exit()
    {
        var result = MessageBox.Show("Are you sure you want to exit TFS Viewer?", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            Application.Current?.Shutdown();
        }
    }
}
