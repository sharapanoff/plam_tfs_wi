using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TfsViewer.App.Contracts;
using TfsViewer.App.Services;
using TfsViewer.App.Infrastructure;
using TfsViewer.App.ViewModels;
using TfsViewer.App.Views;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Services;
using TfsViewer.Core.Infrastructure;
using TfsViewer.Core.Api;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.IO;
using TfsViewer.App.Models;

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

		base.MainWindow = mainWindow;
		mainWindow.Show();

		_ = mainViewModel.InitializeAsync();

		ConfigureTrayIcon(mainWindow, mainViewModel);
	}

	private void ConfigureTrayIcon(MainWindow mainWindow, MainViewModel mainViewModel)
	{
		// Configure tray icon
		if(TryFindResource("TrayIcon") is TaskbarIcon trayIcon)
		{
			trayIcon.DataContext = mainViewModel;
			trayIcon.TrayMouseDoubleClick += (s, args) =>
			{
				mainWindow.Show();
				mainWindow.WindowState = WindowState.Normal;
				mainWindow.Activate();
			};

			// Attempt to load real icon from resources; fallback to generated icon
			try
			{
				var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "app.ico");
				if(File.Exists(iconPath))
				{
					using var fs = File.OpenRead(iconPath);
					trayIcon.Icon = new Icon(fs);
				}
				else
				{
					trayIcon.Icon = CreateFallbackIcon();
				}
			}
			catch
			{
				trayIcon.Icon = CreateFallbackIcon();
			}
		}
	}

	private void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IConfigStore, ConfigStore>();
        services.AddSingleton<ILoggingService, LoggingService>();

        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<ITfsService, TfsService>();


        // App config
        services.AddSingleton<AppConfiguration>(sp => sp.GetRequiredService<IConfigStore>().Load());
        services.AddSingleton<IAppConfiguration>(sp => sp.GetRequiredService<AppConfiguration>());
        services.AddSingleton<IBrowserConfiguration>(sp => sp.GetRequiredService<AppConfiguration>());
        services.AddSingleton<IVsConfiguration>(sp => sp.GetRequiredService<AppConfiguration>());
        services.AddSingleton<ITfsConfiguration>(sp => sp.GetRequiredService<AppConfiguration>());
        

        // ViewModels
        services.AddSingleton<WorkItemsTabViewModel>();
        services.AddSingleton<PullRequestTabViewModel>();
        services.AddSingleton<CodeReviewTabViewModel>();
        services.AddSingleton<MainViewModel>();
        
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

    private Icon CreateFallbackIcon()
    {
        using var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(0x00, 0x7A, 0xCC)); // VS blue
            using var font = new Font(FontFamily.GenericSansSerif, 8, System.Drawing.FontStyle.Bold);
            var text = "TV";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.White, (16 - size.Width) / 2, (16 - size.Height) / 2);
        }
        // Convert bitmap to icon
        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // store as PNG and wrap as icon container
        ms.Position = 0;
        // Simplified: create icon from bitmap handle
        return Icon.FromHandle(bmp.GetHicon());
    }
}


