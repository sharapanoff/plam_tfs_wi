using System.Windows;

namespace TfsViewer.App.Views;

/// <summary>
/// Dialog shown when Visual Studio is not detected on the system
/// </summary>
public partial class VsDetectionErrorDialog : Window
{
    public VsDetectionErrorDialog()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}