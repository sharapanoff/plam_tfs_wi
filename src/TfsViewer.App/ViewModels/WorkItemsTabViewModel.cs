using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TfsViewer.App.Services;
using TfsViewer.Core.Contracts;
using System.Threading;

namespace TfsViewer.App.ViewModels;

/// <summary>
/// ViewModel for the Work Items tab
/// </summary>
public partial class WorkItemsTabViewModel : ObservableObject
{
    private readonly ITfsService _tfsService;
    private readonly ILauncherService _launcherService;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty]
    private ObservableCollection<WorkItemViewModel> _workItems = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private WorkItemViewModel? _selectedWorkItem;

    public int WorkItemCount => WorkItems.Count;

    public WorkItemsTabViewModel(ITfsService tfsService, ILauncherService launcherService)
    {
        _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
    }

    [RelayCommand]
    public async Task LoadWorkItemsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        try
        {
            var workItems = await _tfsService.GetAssignedWorkItemsAsync(_loadCts.Token);
            WorkItems.Clear();

            foreach (var item in workItems)
            {
                WorkItems.Add(WorkItemViewModel.FromModel(item));
            }

            OnPropertyChanged(nameof(WorkItemCount));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load work items: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            _loadCts?.Dispose();
            _loadCts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _loadCts?.Cancel();
    }

    private bool CanCancel() => IsLoading;

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void OpenInBrowser(WorkItemViewModel? workItem)
    {
        if (workItem != null && !string.IsNullOrWhiteSpace(workItem.Url))
        {
            _launcherService.OpenInBrowser(workItem.Url);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void OpenInVisualStudio(WorkItemViewModel? workItem)
    {
        if (workItem != null && _tfsService.IsConnected)
        {
            var serverUrl = string.Empty; // Will get from connection later
            _launcherService.OpenWorkItemInVisualStudio(workItem.Id, serverUrl);
        }
    }

    private bool CanOpen(WorkItemViewModel? workItem)
    {
        return workItem != null;
    }
}
