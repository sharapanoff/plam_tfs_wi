using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TfsViewer.App.Services;
using TfsViewer.Core.Contracts;
using System.Threading;

namespace TfsViewer.App.ViewModels;

public partial class CodeReviewTabViewModel : ObservableObject
{
    private readonly ITfsService _tfsService;
    private readonly ILauncherService _launcherService;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty]
    private ObservableCollection<CodeReviewViewModel> _codeReviews = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private CodeReviewViewModel? _selectedCodeReview;

    public int CodeReviewCount => CodeReviews.Count;

    public CodeReviewTabViewModel(ITfsService tfsService, ILauncherService launcherService)
    {
        _tfsService = tfsService ?? throw new ArgumentNullException(nameof(tfsService));
        _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
    }

    [RelayCommand]
    public async Task LoadCodeReviewsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        try
        {
            var reviews = await _tfsService.GetCodeReviewsAsync(_loadCts.Token);
            CodeReviews.Clear();
            foreach (var cr in reviews)
            {
                CodeReviews.Add(CodeReviewViewModel.FromModel(cr));
            }
            OnPropertyChanged(nameof(CodeReviewCount));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load code reviews: {ex.Message}";
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
    private void OpenInBrowser(CodeReviewViewModel? cr)
    {
        if (cr != null && !string.IsNullOrWhiteSpace(cr.Url))
        {
            _launcherService.OpenInBrowser(cr.Url);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void OpenInVisualStudio(CodeReviewViewModel? cr)
    {
        if (cr != null)
        {
            // Pass the review web URL as server context for launcher fallback
            _launcherService.OpenCodeReviewInVisualStudio(cr.Id, cr.Url);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _loadCts?.Cancel();
    }

    private bool CanOpen() => SelectedCodeReview != null;
    private bool CanCancel() => IsLoading;
}
