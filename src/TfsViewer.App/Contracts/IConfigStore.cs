using TfsViewer.App.Models;

namespace TfsViewer.App.Contracts;

/// <summary>
/// Service interface for storing and retrieving configuration securely
/// </summary>
public interface IConfigStore
{
    void Save(AppConfiguration configuration);
    AppConfiguration Load();
    bool HasStoredConfiguration();
}
