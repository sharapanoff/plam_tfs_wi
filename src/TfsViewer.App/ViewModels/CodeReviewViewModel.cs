using CommunityToolkit.Mvvm.ComponentModel;

namespace TfsViewer.App.ViewModels;

public partial class CodeReviewViewModel : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _requester = string.Empty;
    [ObservableProperty] private DateTime? _creationDate;
    [ObservableProperty] private string _status = string.Empty;
    [ObservableProperty] private string _url = string.Empty;

    public static CodeReviewViewModel FromModel(TfsViewer.Core.Models.CodeReview cr)
    {
        return new CodeReviewViewModel
        {
            Id = cr.Id,
            Title = cr.Title ?? string.Empty,
            Requester = cr.RequestedBy ?? string.Empty,
            CreationDate = cr.CreatedDate,
            Status = cr.Status ?? string.Empty,
            Url = cr.Url ?? string.Empty
        };
    }
}
