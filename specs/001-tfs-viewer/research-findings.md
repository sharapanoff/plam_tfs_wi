# WPF MVVM Best Practices Research Findings

**Research Date:** November 25, 2025  
**Target Platform:** WPF Desktop Application (.NET 6+)  
**Purpose:** TFS Work Item Viewer with Auto-Refresh

---

## 1. TFS SDK Selection

### Decision
Use **Microsoft.TeamFoundationServer.Client** and **Microsoft.VisualStudio.Services.Client** NuGet packages for REST API-based integration.

### Rationale
- **Modern REST-based approach**: These packages provide REST API access, which is the current recommended approach by Microsoft
- **Active support**: Microsoft continues to invest in these REST-based clients
- **.NET 6+ compatibility**: Supports .NET Core, .NET 5+, and .NET Framework
- **Modern features**: Supports async/await patterns and modern C# features
- **Better performance**: REST APIs offer better performance than legacy SOAP APIs
- **Version compatibility**: Package version 16.170.x+ supports Azure DevOps Server 2020+ and TFS 2015+

### Alternatives Considered

#### Microsoft.TeamFoundationServer.ExtendedClient (NOT RECOMMENDED)
- **Status**: Legacy SOAP-based API
- **Limitations**:
  - No .NET Standard support (only .NET Framework)
  - No modern authentication methods
  - No async/await patterns
  - Reduced performance
  - Microsoft no longer investing in SOAP APIs
- **Use case**: Only use when REST APIs don't provide specific functionality (e.g., TFVC workspace creation)

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.TeamFoundationServer.Client" Version="19.225.1" />
<PackageReference Include="Microsoft.VisualStudio.Services.Client" Version="19.225.1" />
<PackageReference Include="Microsoft.VisualStudio.Services.InteractiveClient" Version="19.225.1" />
```

### Code Example - Basic Connection
```csharp
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

// For on-premises TFS with Windows Authentication
var collectionUri = new Uri("https://your-tfs-server:8080/tfs/DefaultCollection");
var credentials = new VssClientCredentials(); // Uses default Windows credentials

// Create connection
var connection = new VssConnection(collectionUri, credentials);

// Get Work Item Tracking client
var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
```

---

## 2. Windows Authentication

### Decision
Use **VssClientCredentials** with default Windows credentials for seamless integrated authentication.

### Rationale
- **Seamless integration**: Automatically uses the current user's Windows credentials
- **No credential prompts**: Users don't need to enter credentials
- **Supports domain authentication**: Works with Active Directory/domain-joined machines
- **Kerberos/NTLM support**: Supports standard Windows authentication protocols
- **Best for on-premises**: Ideal for TFS 2015+ and Azure DevOps Server scenarios

### Implementation Best Practices

#### For Desktop Applications (WPF)
```csharp
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

// Integrated Windows Authentication (recommended for desktop apps)
var credentials = new VssClientCredentials(
    new WindowsCredential(false), // Don't prompt for credentials
    new VssFederatedCredential(false), // Don't use federated auth
    CredentialPromptType.DoNotPrompt); // Never prompt

var connection = new VssConnection(serverUri, credentials);
```

#### For WPF Applications (Handle Authentication in STA Thread)
```csharp
async void InitializeTfsConnection()
{
    Uri serverUri = new Uri("https://your-tfs-server:8080/tfs/DefaultCollection");
    
    var credentials = new VssClientCredentials(
        new WindowsCredential(false),
        new VssFederatedCredential(false),
        CredentialPromptType.PromptIfNeeded);

    VssConnection connection = new VssConnection(serverUri, credentials);
    
    CancellationTokenSource source = new CancellationTokenSource();
    CancellationToken token = source.Token;
    
    // Authenticate asynchronously
    await connection.ConnectAsync(token);
    
    // Now get clients and use them
    var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
}
```

### Server Configuration Requirements
For Windows Authentication to work with TFS:
1. **Enable Windows Authentication in IIS**: TFS site must have Windows Authentication enabled
2. **Valid providers**: NTLM or Kerberos must be configured
3. **Domain membership**: Client machine should be domain-joined or have access to TFS server

### Alternatives Considered

#### Personal Access Tokens (PAT)
```csharp
var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
```
- **Use case**: For testing/development only, not recommended for production desktop apps
- **Limitation**: Requires users to generate and manage tokens

#### Microsoft Entra ID (Azure AD)
```csharp
var credentials = new VssAadCredential();
```
- **Use case**: Primarily for Azure DevOps Services (cloud), not typical for TFS on-premises
- **Limitation**: Requires interactive sign-in

---

## 3. TFS REST API Patterns

### Decision
Use WIQL (Work Item Query Language) for querying work items, and specific REST API clients for pull requests and code reviews.

### Work Items Assigned to Current User

#### Using WIQL Query
```csharp
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

public async Task<IList<WorkItem>> GetMyWorkItemsAsync(
    WorkItemTrackingHttpClient witClient, 
    string projectName)
{
    var wiql = new Wiql
    {
        Query = @"
            SELECT [System.Id], [System.Title], [System.State], 
                   [System.AssignedTo], [System.WorkItemType]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
              AND [System.AssignedTo] = @Me
              AND [System.State] <> 'Closed'
            ORDER BY [System.ChangedDate] DESC"
    };

    var result = await witClient.QueryByWiqlAsync(wiql, projectName);
    
    if (!result.WorkItems.Any())
        return Array.Empty<WorkItem>();

    // Batch retrieve work items (max 200 per batch recommended)
    var ids = result.WorkItems.Select(item => item.Id).ToArray();
    var fields = new[] { "System.Id", "System.Title", "System.State", 
                        "System.AssignedTo", "System.WorkItemType" };
    
    return await witClient.GetWorkItemsAsync(ids, fields);
}
```

### Pull Requests Where User is Reviewer

```csharp
using Microsoft.TeamFoundation.SourceControl.WebApi;

public async Task<List<GitPullRequest>> GetMyPullRequestReviewsAsync(
    GitHttpClient gitClient,
    string projectName,
    string repositoryId)
{
    var searchCriteria = new GitPullRequestSearchCriteria
    {
        Status = PullRequestStatus.Active,
        ReviewerId = currentUserId // Get from authorized identity
    };

    var pullRequests = await gitClient.GetPullRequestsAsync(
        projectName,
        repositoryId,
        searchCriteria);

    return pullRequests;
}

// Alternative: Get all repos and search across them
public async Task<List<GitPullRequest>> GetAllMyPullRequestReviewsAsync(
    GitHttpClient gitClient,
    string projectName)
{
    var allPRs = new List<GitPullRequest>();
    var repositories = await gitClient.GetRepositoriesAsync(projectName);

    foreach (var repo in repositories)
    {
        var searchCriteria = new GitPullRequestSearchCriteria
        {
            Status = PullRequestStatus.Active,
            ReviewerId = currentUserId
        };

        var prs = await gitClient.GetPullRequestsAsync(
            projectName,
            repo.Id,
            searchCriteria);
        
        allPRs.AddRange(prs);
    }

    return allPRs;
}
```

### Code Reviews Assigned to Current User

**Note**: Modern TFS/Azure DevOps uses Pull Requests for code reviews. Legacy code review work items (Code Review Request, Code Review Response) are from older TFS versions.

#### For Modern Pull Request Reviews
Use the pull request API shown above.

#### For Legacy Code Review Work Items (TFS 2012-2015)
```csharp
public async Task<IList<WorkItem>> GetMyCodeReviewsAsync(
    WorkItemTrackingHttpClient witClient,
    string projectName)
{
    var wiql = new Wiql
    {
        Query = @"
            SELECT [System.Id], [System.Title], [System.State]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
              AND [System.WorkItemType] = 'Code Review Request'
              AND [Microsoft.VSTS.Common.ReviewedBy] = @Me
              AND [System.State] <> 'Closed'
            ORDER BY [System.CreatedDate] DESC"
    };

    var result = await witClient.QueryByWiqlAsync(wiql, projectName);
    
    if (!result.WorkItems.Any())
        return Array.Empty<WorkItem>();

    var ids = result.WorkItems.Select(item => item.Id).ToArray();
    return await witClient.GetWorkItemsAsync(ids);
}
```

### Best Practices for Querying

1. **Use @Me macro**: Instead of hardcoding user names, use `@Me` in WIQL queries
2. **Use @project macro**: Use `@project` to scope to current project
3. **Batch retrieval**: Get work item IDs first, then batch retrieve details (max 200 per call)
4. **Specify fields**: Only request fields you need to minimize payload
5. **Use date filtering**: Add date clauses like `[System.ChangedDate] >= @Today-30` for performance
6. **Avoid complex queries**: Minimize `Contains`, `Ever`, and `<>` operators
7. **Server-side pagination**: Use WIQL `TOP` clause for large result sets

---

## 4. Visual Studio Integration

### Decision
Use **custom URL protocols** and **Process.Start** to open TFS work items and pull requests in Visual Studio or web browser.

### Rationale
- **Web-based URLs**: Modern TFS/Azure DevOps primarily uses web UI for viewing work items and pull requests
- **No vs:// protocol support**: Visual Studio doesn't have a built-in `vs://` protocol for opening TFS work items
- **Browser-based viewing**: Most users prefer viewing work items in the browser for full functionality
- **Fallback to Visual Studio**: Can launch Visual Studio with specific solutions/files if needed

### Opening Work Items

#### Option 1: Open in Web Browser (Recommended)
```csharp
public void OpenWorkItemInBrowser(int workItemId, string collectionUrl)
{
    // Format: https://server:8080/tfs/DefaultCollection/_workitems/edit/{id}
    var url = $"{collectionUrl}/_workitems/edit/{workItemId}";
    
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}
```

#### Option 2: Open in Visual Studio (Work Item Form)
```csharp
// Visual Studio can open work items when launched from Team Explorer
// This requires Visual Studio to be installed and connected to TFS

public void OpenWorkItemInVisualStudio(int workItemId, string collectionUrl)
{
    // VS doesn't have a direct protocol, but you can use:
    // 1. mtm:// protocol for Test Manager items (limited)
    // 2. Launch devenv.exe with solution/file
    
    // For work items, web browser is the recommended approach
    var url = $"{collectionUrl}/_workitems/edit/{workItemId}";
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}
```

### Opening Pull Requests

```csharp
public void OpenPullRequestInBrowser(
    string collectionUrl,
    string projectName,
    string repositoryName,
    int pullRequestId)
{
    // Format: https://server/tfs/DefaultCollection/Project/_git/Repo/pullrequest/{id}
    var url = $"{collectionUrl}/{projectName}/_git/{repositoryName}/pullrequest/{pullRequestId}";
    
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}
```

### Opening Files in Visual Studio

```csharp
public void OpenFileInVisualStudio(string filePath)
{
    // Direct file opening
    Process.Start(new ProcessStartInfo
    {
        FileName = "devenv.exe",
        Arguments = $"\"{filePath}\"",
        UseShellExecute = true
    });
}

public void OpenSolutionInVisualStudio(string solutionPath)
{
    Process.Start(new ProcessStartInfo
    {
        FileName = "devenv.exe",
        Arguments = $"\"{solutionPath}\"",
        UseShellExecute = true
    });
}
```

### URL Formats Reference

| Resource Type | URL Format |
|--------------|------------|
| Work Item | `{baseUrl}/_workitems/edit/{id}` |
| Pull Request | `{baseUrl}/{project}/_git/{repo}/pullrequest/{id}` |
| Pull Request (Legacy) | `{baseUrl}/{project}/_git/{repo}/pullrequest/{id}#view=discussion` |
| Code Review (Legacy) | `{baseUrl}/_workitems/edit/{id}` (work item type) |
| File in Repo | `{baseUrl}/{project}/_git/{repo}?path={path}&version=GB{branch}` |

### Alternatives Considered

#### Visual Studio Automation (DTE)
- **Status**: Possible but complex
- **Limitation**: Requires COM automation, version-specific, heavy dependency
- **Use case**: Only if deep Visual Studio integration is required

#### Command Line Parameters
- **Limited support**: `devenv.exe` doesn't directly support TFS work item IDs
- **File-based only**: Can open files and solutions, not TFS resources directly

---

## 5. Performance Considerations

### Decision
Implement batching, pagination, caching, and async patterns for efficient API usage with up to 500 items.

### Batching Strategy

#### Work Item Retrieval (200 per batch)
```csharp
public async Task<List<WorkItem>> GetWorkItemsInBatchesAsync(
    WorkItemTrackingHttpClient witClient,
    IEnumerable<int> workItemIds)
{
    const int batchSize = 200; // Microsoft recommended max
    var allWorkItems = new List<WorkItem>();
    
    var fields = new[] 
    { 
        "System.Id", "System.Title", "System.State",
        "System.AssignedTo", "System.WorkItemType",
        "System.ChangedDate"
    };

    foreach (var batch in workItemIds.Chunk(batchSize))
    {
        try
        {
            var workItems = await witClient.GetWorkItemsAsync(
                ids: batch.ToArray(),
                fields: fields);
            
            allWorkItems.AddRange(workItems);
        }
        catch (Exception ex)
        {
            // Log and continue with next batch
            Debug.WriteLine($"Error fetching batch: {ex.Message}");
        }
    }

    return allWorkItems;
}
```

### Pagination with WIQL

```csharp
public async Task<List<WorkItem>> GetWorkItemsWithPaginationAsync(
    WorkItemTrackingHttpClient witClient,
    string projectName,
    int maxResults = 500)
{
    var wiql = new Wiql
    {
        Query = $@"
            SELECT [System.Id], [System.Title], [System.State]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
              AND [System.AssignedTo] = @Me
              AND [System.State] <> 'Closed'
            ORDER BY [System.ChangedDate] DESC"
    };

    // WIQL supports TOP clause for limiting results
    var result = await witClient.QueryByWiqlAsync(wiql, projectName, top: maxResults);
    
    // Then batch retrieve the items
    return await GetWorkItemsInBatchesAsync(witClient, 
        result.WorkItems.Select(w => w.Id));
}
```

### Caching Strategy

```csharp
using System.Runtime.Caching;

public class TfsDataCache
{
    private readonly MemoryCache _cache = MemoryCache.Default;
    private readonly CacheItemPolicy _defaultPolicy = new CacheItemPolicy
    {
        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
    };

    public async Task<List<WorkItem>> GetWorkItemsWithCacheAsync(
        string cacheKey,
        Func<Task<List<WorkItem>>> fetchFunction)
    {
        // Check cache first
        if (_cache.Contains(cacheKey))
        {
            return _cache.Get(cacheKey) as List<WorkItem>;
        }

        // Fetch from API
        var items = await fetchFunction();

        // Store in cache
        _cache.Set(cacheKey, items, _defaultPolicy);

        return items;
    }

    public void InvalidateCache(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }
}

// Usage
var cache = new TfsDataCache();
var workItems = await cache.GetWorkItemsWithCacheAsync(
    "my-work-items",
    async () => await GetMyWorkItemsAsync(witClient, projectName));
```

### Optimized Query Patterns

#### Use Specific Fields Only
```csharp
// BAD - retrieves all fields
var workItems = await witClient.GetWorkItemsAsync(ids);

// GOOD - retrieve only needed fields
var fields = new[] { "System.Id", "System.Title", "System.State" };
var workItems = await witClient.GetWorkItemsAsync(ids, fields);
```

#### Date-Based Filtering
```csharp
// Limit to recent items for better performance
var wiql = new Wiql
{
    Query = @"
        SELECT [System.Id]
        FROM WorkItems
        WHERE [System.AssignedTo] = @Me
          AND [System.ChangedDate] >= @Today-30
        ORDER BY [System.ChangedDate] DESC"
};
```

#### Avoid Expensive Operators
```csharp
// AVOID: Contains operator (slow)
WHERE [System.Title] CONTAINS 'bug'

// BETTER: Contains Words (faster)
WHERE [System.Title] CONTAINS WORDS 'bug'

// AVOID: <> operator (slow)
WHERE [System.State] <> 'Closed'

// BETTER: IN operator
WHERE [System.State] IN ('Active', 'Resolved', 'New')
```

### Async/Await Best Practices

```csharp
// Use ConfigureAwait(false) in library code
public async Task<WorkItem> GetWorkItemAsync(int id)
{
    return await witClient.GetWorkItemAsync(id).ConfigureAwait(false);
}

// Use Task.WhenAll for parallel operations
public async Task<List<WorkItem>> GetMultipleWorkItemsAsync(
    WorkItemTrackingHttpClient witClient,
    List<int> ids)
{
    var tasks = ids.Select(id => witClient.GetWorkItemAsync(id));
    var results = await Task.WhenAll(tasks);
    return results.ToList();
}
```

### Rate Limiting and Retry Logic

```csharp
using Polly;
using Polly.Retry;

public class TfsClientWithRetry
{
    private readonly AsyncRetryPolicy _retryPolicy;

    public TfsClientWithRetry()
    {
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Debug.WriteLine(
                        $"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });
    }

    public async Task<WorkItem> GetWorkItemWithRetryAsync(
        WorkItemTrackingHttpClient witClient, 
        int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await witClient.GetWorkItemAsync(id);
        });
    }
}
```

### Performance Monitoring

```csharp
using System.Diagnostics;

public async Task<List<WorkItem>> GetWorkItemsWithTimingAsync(
    WorkItemTrackingHttpClient witClient,
    string projectName)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        var items = await GetMyWorkItemsAsync(witClient, projectName);
        
        stopwatch.Stop();
        Debug.WriteLine($"Query completed in {stopwatch.ElapsedMilliseconds}ms, " +
                       $"returned {items.Count} items");
        
        return items;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        Debug.WriteLine($"Query failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
        throw;
    }
}
```

### Best Practices Summary

1. **Batching**: Retrieve work items in batches of 200
2. **Pagination**: Use WIQL `TOP` clause to limit results
3. **Caching**: Cache frequently accessed data for 5-10 minutes
4. **Field Selection**: Only request fields you need
5. **Date Filtering**: Use recent date ranges for better performance
6. **Async Operations**: Use async/await throughout
7. **Parallel Requests**: Use `Task.WhenAll` for independent operations
8. **Retry Logic**: Implement exponential backoff for transient failures
9. **Connection Pooling**: Reuse `VssConnection` and HTTP clients
10. **Monitor Performance**: Track query execution times

### Handling 500 Items

```csharp
public async Task<List<WorkItem>> GetTop500WorkItemsAsync(
    WorkItemTrackingHttpClient witClient,
    string projectName)
{
    // WIQL query limited to 500 items
    var wiql = new Wiql
    {
        Query = @"
            SELECT [System.Id]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
              AND [System.AssignedTo] = @Me
            ORDER BY [System.ChangedDate] DESC"
    };

    // Limit to 500
    var result = await witClient.QueryByWiqlAsync(wiql, projectName, top: 500);
    
    // Batch retrieve in groups of 200
    return await GetWorkItemsInBatchesAsync(witClient, 
        result.WorkItems.Select(w => w.Id));
}
```

---

## Summary of Recommendations

| Area | Recommendation | Rationale |
|------|---------------|-----------|
| **SDK** | Microsoft.TeamFoundationServer.Client + Microsoft.VisualStudio.Services.Client | Modern REST API, .NET 6+ support, active development |
| **Authentication** | VssClientCredentials with Windows integrated auth | Seamless for desktop apps, no prompts, domain integration |
| **Work Items** | WIQL queries with batched retrieval | Flexible querying, efficient batching (200 per call) |
| **Pull Requests** | GitHttpClient with search criteria | Direct PR API, supports filtering by reviewer |
| **Code Reviews** | Pull Request API (modern), Work Item queries (legacy) | PRs are modern approach, legacy uses work items |
| **Visual Studio** | Web URLs + Process.Start | Cross-platform, reliable, user-preferred |
| **Performance** | Batching (200), caching (5min), async/await, retry logic | Handles 500 items efficiently, resilient |

---

## Additional Resources

- [.NET Client Libraries for Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries)
- [.NET Client Library Samples](https://learn.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples)
- [Azure DevOps REST API Reference](https://learn.microsoft.com/en-us/rest/api/azure/devops/)
- [Work Item Query Language (WIQL)](https://learn.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax)
- [Integration Best Practices](https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/integration-bestpractices)
