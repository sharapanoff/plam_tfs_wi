using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using TfsViewer.Core.Api;
using TfsViewer.Core.Contracts;
using TfsViewer.Core.Exceptions;
using TfsViewer.Core.Models;
using TfsWorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;

using TfsViewer.Core.Infrastructure;
namespace TfsViewer.Core.Services;

/// <summary>
/// TFS service implementation for connecting and querying TFS/Azure DevOps
/// </summary>
public class TfsService : ITfsService, IDisposable
{
    private readonly ICacheService _cacheService;
    private readonly ILoggingService? _logging;
    private TfsApiClient? _apiClient;
    private IConstsTFS? _constsTFS;
    private string? _currentUser;

    public TfsService(ICacheService cacheService, ILoggingService? logging = null)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logging = logging;
    }

    public bool IsConnected => _apiClient?.GetConnection() != null;

    public string? CurrentUser => _currentUser;

    public async Task<ConnectionResult> ConnectAsync(IConstsTFS credentials, CancellationToken cancellationToken = default)
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

            _constsTFS = credentials;

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
        if (!IsConnected || _constsTFS == null)
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
        _constsTFS = null;
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

        // Create timeout token (30 seconds)
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

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

            // WIQL query to get assigned work items (exclude DevNotes and Code Review types)
            var query = $@"
                SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State], 
                       [System.AssignedTo], [System.CreatedDate], [System.ChangedDate]
                FROM WorkItems
                WHERE [System.AssignedTo] = @Me
                  AND [System.WorkItemType] <> 'DevNotes'
                  AND [System.WorkItemType] <> 'Code Review Response'
                  AND [System.WorkItemType] <> 'Code Review Request'
                ORDER BY [System.ChangedDate] DESC";

            var wiql = new Wiql { Query = query };
            var retry = RetryPolicy.CreateTfsDefaultPolicy(_logging);
            var queryResult = await retry.ExecuteAsync(ct => witClient.QueryByWiqlAsync(wiql, cancellationToken: ct), timeoutCts.Token);

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

            var tfsWorkItems = await retry.ExecuteAsync(ct => witClient.GetWorkItemsAsync(ids, fields, cancellationToken: ct), timeoutCts.Token);

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
                // Construct proper TFS web UI URL instead of using API URL
                Url = $"{_constsTFS?.ServerUrl}/_workitems/edit/{wi.Id ?? 0}"
            }).ToList();

            // Cache for 5 minutes
            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TfsServiceException("Request timed out after 30 seconds", new TimeoutException())
            {
                ServerUrl = _constsTFS?.ServerUrl,
                Operation = "GetAssignedWorkItems"
            };
        }
        catch (Exception ex) when (ex is not TfsServiceException)
        {
            _logging?.LogError("Failed to retrieve work items", ex);
            throw new TfsServiceException($"Failed to retrieve work items: {ex.Message}", ex)
            {
                ServerUrl = _constsTFS?.ServerUrl,
                Operation = "GetAssignedWorkItems"
            };
        }
    }

    public async Task<IEnumerable<PullRequest>> GetPullRequestsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "assigned_pull_requests";

        // Check cache first (5-minute TTL)
        if (_cacheService.TryGet<List<PullRequest>>(cacheKey, out var cachedItems) && cachedItems != null)
        {
            return cachedItems;
        }

        if (!IsConnected)
        {
            throw new TfsServiceException("Not connected to TFS server") { Operation = "GetPullRequests" };
        }

        // Create timeout token (30 seconds)
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

        try
        {
            var gitClient = _apiClient?.GetGitClient();
            var connection = _apiClient?.GetConnection();
            if (gitClient == null || connection == null)
            {
                throw new TfsServiceException("Git client not available") { Operation = "GetPullRequests" };
            }

            // Enumerate repositories and collect PRs where current user is a reviewer
            var retry = RetryPolicy.CreateTfsDefaultPolicy(_logging);
            var result = await retry.ExecuteAsync(async ct =>
            {
                var prs = new List<PullRequest>();
                var repos = await gitClient.GetRepositoriesAsync(cancellationToken: ct);
                foreach (var repo in repos)
                {
                    var criteria = new GitPullRequestSearchCriteria
                    {
                        Status = PullRequestStatus.Active
                    };
                    var prList = await gitClient.GetPullRequestsAsync(repo.Id, criteria, cancellationToken: ct);
                    foreach (var pr in prList)
                    {
                        if (pr.Reviewers != null && pr.Reviewers.Any(r => string.Equals(r.DisplayName, _currentUser, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Construct proper TFS web UI URL for pull request
                            var projectName = repo.ProjectReference?.Name ?? "DefaultCollection";
                            var repoName = repo.Name ?? string.Empty;
                            var prUrl = $"{_constsTFS?.ServerUrl}/{projectName}/_git/{repoName}/pullrequest/{pr.PullRequestId}";
                            
                            prs.Add(new PullRequest
                            {
                                Id = pr.PullRequestId,
                                Title = pr.Title ?? string.Empty,
                                CreatedBy = pr.CreatedBy?.DisplayName ?? string.Empty,
                                Repository = repo.Name ?? string.Empty,
                                SourceBranch = pr.SourceRefName ?? string.Empty,
                                TargetBranch = pr.TargetRefName ?? string.Empty,
                                CreatedDate = pr.CreationDate,
                                Status = pr.Status.ToString(),
                                Url = prUrl,
                                IsDraft = pr.IsDraft ?? false
                            });
                        }
                    }
                }
                return prs;
            }, timeoutCts.Token);

            // Cache for 5 minutes
            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TfsServiceException("Request timed out after 30 seconds", new TimeoutException())
            {
                ServerUrl = _constsTFS?.ServerUrl,
                Operation = "GetPullRequests"
            };
        }
        catch (Exception ex) when (ex is not TfsServiceException)
        {
            _logging?.LogError("Failed to retrieve pull requests", ex);
            throw new TfsServiceException($"Failed to retrieve pull requests: {ex.Message}", ex)
            {
                ServerUrl = _constsTFS?.ServerUrl,
                Operation = "GetPullRequests"
            };
        }
    }

    public async Task<IEnumerable<CodeReview>> GetCodeReviewsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "assigned_code_reviews";

        // Check cache first (5-minute TTL)
        if (_cacheService.TryGet<List<CodeReview>>(cacheKey, out var cachedItems) && cachedItems != null)
        {
            return cachedItems;
        }

        if (!IsConnected)
        {
            throw new TfsServiceException("Not connected to TFS server") { Operation = "GetCodeReviews" };
        }

        // Create timeout token (30 seconds)
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

        try
        {
            var witClient = _apiClient?.GetWorkItemClient();
            if (witClient == null)
            {
                throw new TfsServiceException("Work item client not available") { Operation = "GetCodeReviews" };
            }

            // WIQL query to get all work items with Type='Code Review Response' assigned to current user
            var query = $@"
                SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State], 
                       [System.AssignedTo], [System.CreatedDate], [System.ChangedDate]
                FROM WorkItems
                WHERE [System.WorkItemType] = 'Code Review Response'
                  AND [System.AssignedTo] = @Me
                ORDER BY [System.ChangedDate] DESC";

            var wiql = new Wiql { Query = query };
            var retry = RetryPolicy.CreateTfsDefaultPolicy(_logging);
            var queryResult = await retry.ExecuteAsync(ct => witClient.QueryByWiqlAsync(wiql, cancellationToken: ct), timeoutCts.Token);

            if (queryResult.WorkItems == null || !queryResult.WorkItems.Any())
            {
                var emptyList = new List<CodeReview>();
                _cacheService.Set(cacheKey, emptyList, TimeSpan.FromMinutes(5));
                return emptyList;
            }

            var ids = queryResult.WorkItems.Select(wi => wi.Id).ToArray();
            var fields = new[] 
            { 
                "System.Id", "System.Title", "System.WorkItemType", "System.State",
                "System.AssignedTo", "System.CreatedDate", "System.ChangedDate"
            };

            var tfsWorkItems = await retry.ExecuteAsync(ct => witClient.GetWorkItemsAsync(ids, fields, cancellationToken: ct), timeoutCts.Token);

            var result = tfsWorkItems.Select(wi => new CodeReview
            {
                Id = wi.Id ?? 0,
                Title = wi.Fields.ContainsKey("System.Title") ? wi.Fields["System.Title"]?.ToString() ?? string.Empty : string.Empty,
                RequestedBy = wi.Fields.ContainsKey("System.AssignedTo") ? wi.Fields["System.AssignedTo"]?.ToString() ?? string.Empty : string.Empty,
                CreatedDate = wi.Fields.ContainsKey("System.CreatedDate") ? wi.Fields["System.CreatedDate"] as DateTime? ?? DateTime.MinValue : DateTime.MinValue,
                Status = wi.Fields.ContainsKey("System.State") ? wi.Fields["System.State"]?.ToString() ?? string.Empty : string.Empty,
                // Construct proper TFS web UI URL instead of using API URL
                Url = $"{_constsTFS?.ServerUrl}/_workitems/edit/{wi.Id ?? 0}"
            }).ToList();

            // Cache for 5 minutes
            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TfsServiceException("Request timed out after 30 seconds", new TimeoutException())
            {
                ServerUrl = _constsTFS?.ServerUrl,
                Operation = "GetCodeReviews"
            };
        }
        catch (Exception ex) when (ex is not TfsServiceException)
        {
            _logging?.LogError("Failed to retrieve code reviews", ex);
            throw new TfsServiceException($"Failed to retrieve code reviews: {ex.Message}", ex)
            {
                ServerUrl = _constsTFS?.ServerUrl,
                Operation = "GetCodeReviews"
            };
        }
    }

    public void Dispose()
    {
        Disconnect();
    }
}
