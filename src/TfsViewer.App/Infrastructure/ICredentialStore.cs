using TfsViewer.Core.Models;

namespace TfsViewer.Core.Contracts;

/// <summary>
/// Service interface for storing and retrieving credentials securely
/// </summary>
public interface ICredentialStore
{
    void SaveCredentials(CConsts credentials);
    
    CConsts? LoadCredentials();
    
    void ClearCredentials();
    
    bool HasStoredCredentials();
}
