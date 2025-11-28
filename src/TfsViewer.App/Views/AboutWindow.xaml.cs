using System.Windows;
using TfsViewer.App.ViewModels;

namespace TfsViewer.App.Views;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = new AboutViewModel(this);
    }
}