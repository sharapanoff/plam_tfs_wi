using CommunityToolkit.Mvvm.ComponentModel;
using TfsViewer.Core.Models;

namespace TfsViewer.App.ViewModels;

/// <summary>
/// ViewModel for displaying a single work item
/// </summary>
public partial class WorkItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _workItemType = string.Empty;

    [ObservableProperty]
    private string _state = string.Empty;

    [ObservableProperty]
    private DateTime? _assignedDate;

    [ObservableProperty]
    private string _url = string.Empty;

    public string AssignedDateWithTime => AssignedDate?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

    public static WorkItemViewModel FromModel(WorkItem model)
    {
        return new WorkItemViewModel
        {
            Id = model.Id,
            Title = model.Title,
            WorkItemType = model.WorkItemType,
            State = model.State,
            AssignedDate = model.AssignedDate ?? model.ChangedDate,
            Url = model.Url
        };
    }
}
