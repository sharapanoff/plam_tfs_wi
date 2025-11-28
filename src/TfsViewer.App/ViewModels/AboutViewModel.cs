using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TfsViewer.App.ViewModels;

/// <summary>
/// ViewModel for the About dialog
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    private readonly Window _window;

    [ObservableProperty]
    private string _version;

    public AboutViewModel(Window window)
    {
        _window = window;
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        Version = $"Version {version?.ToString() ?? "1.0.0.0"}";
    }

    [RelayCommand]
    private void Close()
    {
        _window.Close();
    }
}