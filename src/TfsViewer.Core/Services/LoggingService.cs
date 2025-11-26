using Serilog;
using Serilog.Events;
using Serilog.Sinks.File; // Add this using directive

namespace TfsViewer.Core.Services;

public interface ILoggingService
{
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}

public class LoggingService : ILoggingService
{
    private static readonly Lazy<ILogger> _logger = new(() =>
    {
        var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TfsViewer", "logs");
        Directory.CreateDirectory(logDir);

        return new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.File(
                path: Path.Combine(logDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Warning,
                retainedFileCountLimit: 14)
            .CreateLogger();
    });

    private ILogger Logger => _logger.Value;

    public void LogWarning(string message)
    {
        Logger.Warning(message);
    }

    public void LogError(string message, Exception? ex = null)
    {
        if (ex != null)
            Logger.Error(ex, message);
        else
            Logger.Error(message);
    }
}
