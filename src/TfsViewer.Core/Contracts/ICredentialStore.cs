using TfsViewer.Core.Models;

namespace TfsViewer.Core.Contracts;

/// <summary>
/// Service interface for storing and retrieving credentials securely
/// </summary>
public interface ICredentialStore
{
    void SaveCredentials(TfsCredentials credentials);
    
    TfsCredentials? LoadCredentials();
    
    void ClearCredentials();
    
    bool HasStoredCredentials();
}
