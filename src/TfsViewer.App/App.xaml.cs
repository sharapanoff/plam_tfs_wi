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
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<ITfsService, TfsService>();
        services.AddTransient<TfsApiClient>();

        // App services
        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<Configuration>(_ => Configuration.Load());

        // ViewModels
        services.AddSingleton<WorkItemsTabViewModel>();
        services.AddSingleton<MainViewModel>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public T? GetService<T>() where T : class => _serviceProvider?.GetService<T>();
}


