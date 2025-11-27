using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TfsViewer.App.Services;
using TfsViewer.App.Infrastructure;
using TfsViewer.App.ViewModels;
using TfsViewer.App.Views;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Services;
using TfsViewer.Core.Infrastructure;
using TfsViewer.Core.Api;
using Hardcodet.Wpf.TaskbarNotification;

namespace TfsViewer.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Show main window
        var mainWindow = new MainWindow();
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        
        MainWindow = mainWindow;
        mainWindow.Show();

        // Initialize async (load credentials and data)
        _ = mainViewModel.InitializeAsync();

        // Configure tray icon
        if (TryFindResource("TrayIcon") is TaskbarIcon trayIcon)
        {
            trayIcon.DataContext = mainViewModel;
            trayIcon.TrayMouseDoubleClick += (s, args) =>
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            };
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<ITfsService>(sp =>
        {
            var cache = sp.GetRequiredService<ICacheService>();
            var log = sp.GetService<ILoggingService>();
            return new TfsService(cache, log);
        });
        services.AddTransient<TfsApiClient>();

        // App services
        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<Configuration>(_ => Configuration.Load());

        // ViewModels
        services.AddSingleton<WorkItemsTabViewModel>();
        services.AddSingleton<PullRequestTabViewModel>(sp =>
        {
            var tfs = sp.GetRequiredService<ITfsService>();
            var launcher = sp.GetRequiredService<ILauncherService>();
            var logging = sp.GetService<ILoggingService>();
            return new PullRequestTabViewModel(tfs, launcher, logging);
        });
        services.AddSingleton<CodeReviewTabViewModel>(sp =>
        {
            var tfs = sp.GetRequiredService<ITfsService>();
            var launcher = sp.GetRequiredService<ILauncherService>();
            var logging = sp.GetService<ILoggingService>();
            return new CodeReviewTabViewModel(tfs, launcher, logging);
        });
        services.AddSingleton<MainViewModel>(sp =>
        {
            return new MainViewModel(
                sp.GetRequiredService<ITfsService>(),
                sp.GetRequiredService<ICredentialStore>(),
                sp.GetRequiredService<Configuration>(),
                sp.GetRequiredService<ICacheService>(),
                sp.GetRequiredService<WorkItemsTabViewModel>(),
                sp.GetRequiredService<PullRequestTabViewModel>(),
                sp.GetRequiredService<CodeReviewTabViewModel>()
            );
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (TryFindResource("TrayIcon") is TaskbarIcon trayIcon)
        {
            trayIcon.Dispose();
        }
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public T? GetService<T>() where T : class => _serviceProvider?.GetService<T>();
}


