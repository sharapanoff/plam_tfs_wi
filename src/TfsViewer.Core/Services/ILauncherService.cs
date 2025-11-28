using System.Threading.Tasks;

namespace TfsViewer.Services;

public interface ILauncherService
{
    bool IsVisualStudioInstalled { get; }
    Task LaunchVisualStudioAsync(string url);
}