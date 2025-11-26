using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace TfsViewer.Core.Api;

/// <summary>
/// Low-level TFS REST API client wrapper
/// </summary>
public class TfsApiClient : IDisposable
{
    private VssConnection? _connection;
    private WorkItemTrackingHttpClient? _witClient;

    public async Task<bool> ConnectAsync(string serverUrl, VssCredentials credentials, CancellationToken cancellationToken = default)
    {
        try
        {
            _connection = new VssConnection(new Uri(serverUrl), credentials);
            await _connection.ConnectAsync(cancellationToken);
            _witClient = _connection.GetClient<WorkItemTrackingHttpClient>();
            return true;
        }
        catch
        {
            Dispose();
            return false;
        }
    }

    public WorkItemTrackingHttpClient? GetWorkItemClient() => _witClient;

    public VssConnection? GetConnection() => _connection;

    public void Dispose()
    {
        _witClient?.Dispose();
        _connection?.Dispose();
        _witClient = null;
        _connection = null;
    }
}
