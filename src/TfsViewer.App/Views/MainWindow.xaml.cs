using System.Windows;
using System.Windows.Input;
using TfsViewer.App.ViewModels;

namespace TfsViewer.App.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StateChanged += OnStateChangedMinimizeHide;
    }

    private void OnStateChangedMinimizeHide(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    private void TitleTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //if (e.ClickCount == 2) // Double-click
        {
            if (sender is FrameworkElement element && element.DataContext is CodeReviewViewModel codeReview)
            {
                codeReview.ToggleTitleExpansion();
            }
        }
    }
}
