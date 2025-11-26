namespace TfsViewer.Core.Exceptions;

/// <summary>
/// Exception thrown when TFS service operations fail
/// </summary>
public class TfsServiceException : Exception
{
    public TfsServiceException()
    {
    }

    public TfsServiceException(string message) 
        : base(message)
    {
    }

    public TfsServiceException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public string? ServerUrl { get; init; }
    
    public string? Operation { get; init; }
}
