using CommunityToolkit.Mvvm.ComponentModel;

namespace TfsViewer.App.ViewModels;

public partial class CodeReviewViewModel : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _shortenedTitle = string.Empty;
    [ObservableProperty] private bool _isTitleExpanded = false;
    [ObservableProperty] private string _requester = string.Empty;
    [ObservableProperty] private DateTime? _creationDate;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _projectName = string.Empty;
    [ObservableProperty] private string _areaPath = string.Empty;

    public string CreationDateWithTime => CreationDate?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

    public string DisplayTitle => IsTitleExpanded ? Title : ShortenedTitle;

    public string ProjectAreaDisplay => $"{AreaPath ?? string.Empty}\n**{ProjectName ?? string.Empty}**";

    public void ToggleTitleExpansion()
    {
        IsTitleExpanded = !IsTitleExpanded;
    }

    public static CodeReviewViewModel FromModel(TfsViewer.Core.Models.CodeReview cr)
    {
        var vm = new CodeReviewViewModel
        {
            Id = cr.Id,
            Title = cr.Title ?? string.Empty,
            Requester = cr.RequestedBy ?? string.Empty,
            CreationDate = cr.CreatedDate,
            Status = cr.Status ?? string.Empty,
            Url = cr.Url ?? string.Empty,
            ProjectName = cr.ProjectName ?? string.Empty,
            AreaPath = cr.AreaPath ?? string.Empty
        };

        // Create shortened title (first 50 characters + ...)
        vm.ShortenedTitle = cr.Title?.Length > 50 
            ? cr.Title.Substring(0, 47) + "..." 
            : cr.Title ?? string.Empty;

        return vm;
    }

    // Ensure UI updates when dependent properties change
    partial void OnIsTitleExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnTitleChanged(string value)
    {
        // Recompute shortened title if title changes and notify display
        ShortenedTitle = value?.Length > 50 ? value.Substring(0, 47) + "..." : value ?? string.Empty;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnShortenedTitleChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }
}
