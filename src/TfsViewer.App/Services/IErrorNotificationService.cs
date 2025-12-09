namespace TfsViewer.App.Services;

using TfsViewer.App.ViewModels;

/// <summary>
/// Service for notifying errors and status messages through the main view model
/// instead of using MessageBox dialogs
/// </summary>
public interface IErrorNotificationService
{
    void Create(MainViewModel mainViewModel);

    /// <summary>
    /// Shows an error message through the main view model status bar
    /// </summary>
    /// <param name="message">The error message to display</param>
    void ShowError(string message);

    /// <summary>
    /// Shows a success message through the main view model status bar
    /// </summary>
    /// <param name="message">The success message to display</param>
    void ShowSuccess(string message);

    /// <summary>
    /// Shows a general status message through the main view model status bar
    /// </summary>
    /// <param name="message">The status message to display</param>
    void ShowStatus(string message);
}

/// <summary>
/// Implementation of error notification service using MainViewModel's StatusMessage
/// </summary>
public class ErrorNotificationService : IErrorNotificationService
{
    private MainViewModel _mainViewModel;


    public void Create(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
    }

    public void ShowError(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        _mainViewModel.StatusMessage = $"Error: {message}";
    }

    public void ShowSuccess(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        _mainViewModel.StatusMessage = message;
    }

    public void ShowStatus(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        _mainViewModel.StatusMessage = message;
    }
}
