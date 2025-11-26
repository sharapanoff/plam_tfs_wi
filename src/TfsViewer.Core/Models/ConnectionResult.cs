namespace TfsViewer.Core.Models;

/// <summary>
/// Represents the result of a TFS connection attempt
/// </summary>
public class ConnectionResult
{
    public bool Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public string? ServerVersion { get; set; }
    
    public string? AuthenticatedUser { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public static ConnectionResult SuccessResult(string serverVersion, string authenticatedUser)
    {
        return new ConnectionResult
        {
            Success = true,
            ServerVersion = serverVersion,
            AuthenticatedUser = authenticatedUser
        };
    }
    
    public static ConnectionResult FailureResult(string errorMessage)
    {
        return new ConnectionResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
