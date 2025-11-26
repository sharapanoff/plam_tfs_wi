using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using TfsViewer.Core.Api;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Exceptions;
using TfsViewer.Core.Models;
using TfsWorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

namespace TfsViewer.Core.Services;

/// <summary>
/// TFS service implementation for connecting and querying TFS/Azure DevOps
/// </summary>
public class TfsService : ITfsService, IDisposable
{
    private readonly ICacheService _cacheService;
    private TfsApiClient? _apiClient;
    private TfsCredentials? _credentials;
    private string? _currentUser;

    public TfsService(ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public bool IsConnected => _apiClient?.GetConnection() != null;

    public string? CurrentUser => _currentUser;

    public async Task<ConnectionResult> ConnectAsync(TfsCredentials credentials, CancellationToken cancellationToken = default)
    {
        if (credentials == null)
            throw new ArgumentNullException(nameof(credentials));

        if (string.IsNullOrWhiteSpace(credentials.ServerUrl))
            return ConnectionResult.FailureResult("Server URL is required");

        try
        {
            Disconnect();

            _apiClient = new TfsApiClient();
            VssCredentials vssCredentials;

            if (credentials.UseWindowsAuthentication)
            {
                vssCredentials = new VssCredentials();
            }
            else if (!string.IsNullOrWhiteSpace(credentials.PersonalAccessToken))
            {
                vssCredentials = new VssBasicCredential(string.Empty, credentials.PersonalAccessToken);
            }
            else
            {
                return ConnectionResult.FailureResult("No valid authentication method provided");
            }

            var connected = await _apiClient.ConnectAsync(credentials.ServerUrl, vssCredentials, cancellationToken);

            if (!connected)
            {
                return ConnectionResult.FailureResult("Failed to connect to TFS server");
            }

            _credentials = credentials;

            // Get authenticated user
            var connection = _apiClient.GetConnection();
            if (connection != null)
            {
                await connection.ConnectAsync(cancellationToken);
                _currentUser = connection.AuthorizedIdentity?.DisplayName ?? "Unknown";
            }

            return ConnectionResult.SuccessResult("TFS/Azure DevOps", _currentUser ?? "Unknown");
        }
        catch (Exception ex)
        {
            Disconnect();
            return ConnectionResult.FailureResult($"Connection failed: {ex.Message}");
        }
    }

    public async Task<ConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _credentials == null)
        {
            return ConnectionResult.FailureResult("Not connected");
        }

        try
        {
            var witClient = _apiClient?.GetWorkItemClient();
            if (witClient == null)
            {
                return ConnectionResult.FailureResult("Work item client not available");
            }

            // Simple query to test connection
            var query = "SELECT [System.Id] FROM WorkItems WHERE [System.WorkItemType] <> '' ORDER BY [System.Id] DESC";
            var wiql = new Wiql { Query = query };
            var result = await witClient.QueryByWiqlAsync(wiql, top: 1, cancellationToken: cancellationToken);

            return ConnectionResult.SuccessResult("TFS/Azure DevOps", _currentUser ?? "Unknown");
        }
        catch (Exception ex)
        {
            return ConnectionResult.FailureResult($"Connection test failed: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        _apiClient?.Dispose();
        _apiClient = null;
        _credentials = null;
        _currentUser = null;
        _cacheService.Clear();
    }

    public async Task<IEnumerable<Models.WorkItem>> GetAssignedWorkItemsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "assigned_work_items";

        // Check cache first (5-minute TTL)
        if (_cacheService.TryGet<List<Models.WorkItem>>(cacheKey, out var cachedItems) && cachedItems != null)
        {
            return cachedItems;
        }

        if (!IsConnected)
        {
            throw new TfsServiceException("Not connected to TFS server") 
            { 
                Operation = "GetAssignedWorkItems" 
            };
        }

        try
        {
            var witClient = _apiClient?.GetWorkItemClient();
            if (witClient == null)
            {
                throw new TfsServiceException("Work item client not available") 
                { 
                    Operation = "GetAssignedWorkItems" 
                };
            }

            // WIQL query to get assigned work items
            var query = $@"
                SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State], 
                       [System.AssignedTo], [System.CreatedDate], [System.ChangedDate]
                FROM WorkItems
                WHERE [System.AssignedTo] = @Me
                ORDER BY [System.ChangedDate] DESC";

            var wiql = new Wiql { Query = query };
            var queryResult = await witClient.QueryByWiqlAsync(wiql, cancellationToken: cancellationToken);

            if (queryResult.WorkItems == null || !queryResult.WorkItems.Any())
            {
                var emptyList = new List<Models.WorkItem>();
                _cacheService.Set(cacheKey, emptyList, TimeSpan.FromMinutes(5));
                return emptyList;
            }

            var ids = queryResult.WorkItems.Select(wi => wi.Id).ToArray();
            var fields = new[] 
            { 
                "System.Id", "System.Title", "System.WorkItemType", "System.State",
                "System.AssignedTo", "System.CreatedDate", "System.ChangedDate",
                //"System.Priority", "System.AreaPath", "System.IterationPath"
            };

            var tfsWorkItems = await witClient.GetWorkItemsAsync(ids, fields, cancellationToken: cancellationToken);

            var result = tfsWorkItems.Select(wi => new Models.WorkItem
            {
                Id = wi.Id ?? 0,
                Title = wi.Fields.ContainsKey("System.Title") ? wi.Fields["System.Title"]?.ToString() ?? string.Empty : string.Empty,
                WorkItemType = wi.Fields.ContainsKey("System.WorkItemType") ? wi.Fields["System.WorkItemType"]?.ToString() ?? string.Empty : string.Empty,
                State = wi.Fields.ContainsKey("System.State") ? wi.Fields["System.State"]?.ToString() ?? string.Empty : string.Empty,
                AssignedTo = wi.Fields.ContainsKey("System.AssignedTo") ? wi.Fields["System.AssignedTo"]?.ToString() ?? string.Empty : string.Empty,
                CreatedDate = wi.Fields.ContainsKey("System.CreatedDate") ? wi.Fields["System.CreatedDate"] as DateTime? : null,
                ChangedDate = wi.Fields.ContainsKey("System.ChangedDate") ? wi.Fields["System.ChangedDate"] as DateTime? : null,
                AssignedDate = wi.Fields.ContainsKey("System.ChangedDate") ? wi.Fields["System.ChangedDate"] as DateTime? : null,
                Priority = wi.Fields.ContainsKey("System.Priority") ? wi.Fields["System.Priority"]?.ToString() ?? string.Empty : string.Empty,
                AreaPath = wi.Fields.ContainsKey("System.AreaPath") ? wi.Fields["System.AreaPath"]?.ToString() ?? string.Empty : string.Empty,
                IterationPath = wi.Fields.ContainsKey("System.IterationPath") ? wi.Fields["System.IterationPath"]?.ToString() ?? string.Empty : string.Empty,
                Url = wi.Url ?? string.Empty
            }).ToList();

            // Cache for 5 minutes
            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        catch (Exception ex) when (ex is not TfsServiceException)
        {
            throw new TfsServiceException($"Failed to retrieve work items: {ex.Message}", ex)
            {
                ServerUrl = _credentials?.ServerUrl,
                Operation = "GetAssignedWorkItems"
            };
        }
    }

    public Task<IEnumerable<PullRequest>> GetPullRequestsAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder - will be implemented in User Story 2
        return Task.FromResult(Enumerable.Empty<PullRequest>());
    }

    public Task<IEnumerable<CodeReview>> GetCodeReviewsAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder - will be implemented in User Story 3
        return Task.FromResult(Enumerable.Empty<CodeReview>());
    }

    public void Dispose()
    {
        Disconnect();
    }
}
