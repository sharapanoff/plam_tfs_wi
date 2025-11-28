using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TfsViewer.App.Services;
using TfsViewer.Core.Contracts;
using System.Threading;
using TfsViewer.Core.Services;

namespace TfsViewer.App.ViewModels;

public partial class PullRequestTabViewModel : ObservableObject
{
    private readonly ITfsService _tfsService;
    private readonly ILauncherService _launcherService;
    private readonly ILoggingService? _logging;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty]
    private ObservableCollection<PullRequestViewModel> _pullRequests = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private PullRequestViewModel? _selectedPullRequest;

    public int PullRequestCount => PullRequests.Count;

    public PullRequestTabViewModel(ITfsService tfsService, ILauncherService launcherService, ILoggingService? logging = null)
    {
        _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
        _logging = logging;
    }

    [RelayCommand]
    public async Task LoadPullRequestsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        try
        {
            var prs = await _tfsService.GetPullRequestsAsync(_loadCts.Token);
            PullRequests.Clear();
            foreach (var pr in prs)
            {
                PullRequests.Add(PullRequestViewModel.FromModel(pr));
            }
            OnPropertyChanged(nameof(PullRequestCount));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load pull requests: {ex.Message}";
            _logging?.LogError("Failed to load pull requests", ex);
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            _loadCts?.Dispose();
            _loadCts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void OpenInBrowser(PullRequestViewModel? pr)
    {
        if (pr != null && !string.IsNullOrWhiteSpace(pr.Url))
        {
            _launcherService.OpenInBrowser(pr.Url);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void OpenInVisualStudio(PullRequestViewModel? pr)
    {
        if (pr != null)
        {
            try
            {
                // Pass the PR web URL as repository context for launcher fallback
                _launcherService.OpenPullRequestInVisualStudio(pr.Id, pr.Url);
            }
            catch (InvalidOperationException)
            {
                // Show VS detection error dialog
                var dialog = new Views.VsDetectionErrorDialog();
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open in Visual Studio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _loadCts?.Cancel();
    }

    private bool CanOpen(PullRequestViewModel? workItem)
    {
        return workItem != null;
    }

    private bool CanCancel() => IsLoading;
}
