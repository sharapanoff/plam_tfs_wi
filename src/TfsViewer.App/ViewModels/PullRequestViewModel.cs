using CommunityToolkit.Mvvm.ComponentModel;

namespace TfsViewer.App.ViewModels;

public partial class PullRequestViewModel : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _author = string.Empty;
    [ObservableProperty] private DateTime? _creationDate;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private string _url = string.Empty;

    public string CreationDateWithTime => CreationDate?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

    public static PullRequestViewModel FromModel(TfsViewer.Core.Models.PullRequest pr)
    {
        return new PullRequestViewModel
        {
            Id = pr.Id,
            Title = pr.Title ?? string.Empty,
            Author = pr.CreatedBy ?? string.Empty,
            CreationDate = pr.CreatedDate,
            Status = pr.Status ?? string.Empty,
            Url = pr.Url ?? string.Empty
        };
    }
}
