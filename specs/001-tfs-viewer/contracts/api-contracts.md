# API Contracts: TFS Read-Only Viewer

**Feature**: 001-tfs-viewer  
**Date**: 2025-11-25  
**API Type**: TFS/Azure DevOps REST API v6.0

## Overview

This document defines the service interfaces and API contracts for interacting with TFS Server. All operations are read-only.

---

## Service Interfaces

### ITfsService

Primary service interface for TFS data retrieval.

```csharp
namespace TfsViewer.Core.Services
{
    /// <summary>
    /// Service for retrieving data from TFS Server
    /// </summary>
    public interface ITfsService
    {
        /// <summary>
        /// Connects to TFS server and validates credentials
        /// </summary>
        /// <param name="serverUrl">TFS server URL (e.g., https://tfs.company.com/DefaultCollection)</param>
        /// <param name="credentials">Authentication credentials</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Connection result with status</returns>
        Task<ConnectionResult> ConnectAsync(
            string serverUrl, 
            TfsCredentials credentials,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all work items assigned to the current user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of work items</returns>
        Task<IReadOnlyList<WorkItem>> GetAssignedWorkItemsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all pull requests where current user is a reviewer
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of pull requests</returns>
        Task<IReadOnlyList<PullRequest>> GetPullRequestsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all code reviews assigned to current user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of code reviews</returns>
        Task<IReadOnlyList<CodeReview>> GetCodeReviewsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tests connection to TFS server
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if connected, false otherwise</returns>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from TFS server
        /// </summary>
        void Disconnect();
    }
}
```

---

### ICacheService

Service interface for caching TFS data.

```csharp
namespace TfsViewer.Core.Services
{
    /// <summary>
    /// Service for caching TFS data to improve performance
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets cached items by type
        /// </summary>
        /// <typeparam name="T">Type of items to retrieve</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached items or null if not found/expired</returns>
        IReadOnlyList<T>? Get<T>(string key) where T : class;

        /// <summary>
        /// Stores items in cache with TTL
        /// </summary>
        /// <typeparam name="T">Type of items to store</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="items">Items to cache</param>
        /// <param name="ttl">Time to live</param>
        void Set<T>(string key, IReadOnlyList<T> items, TimeSpan ttl) where T : class;

        /// <summary>
        /// Invalidates cached items by key
        /// </summary>
        /// <param name="key">Cache key</param>
        void Invalidate(string key);

        /// <summary>
        /// Clears all cached data
        /// </summary>
        void Clear();

        /// <summary>
        /// Saves cache to disk for offline access
        /// </summary>
        Task SaveToDiskAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads cache from disk
        /// </summary>
        Task LoadFromDiskAsync(CancellationToken cancellationToken = default);
    }
}
```

---

### ILauncherService

Service interface for opening items in Visual Studio or browser.

```csharp
namespace TfsViewer.App.Services
{
    /// <summary>
    /// Service for launching TFS items in external applications
    /// </summary>
    public interface ILauncherService
    {
        /// <summary>
        /// Opens a work item in Visual Studio
        /// </summary>
        /// <param name="workItem">Work item to open</param>
        /// <returns>True if launched successfully</returns>
        bool OpenWorkItemInVisualStudio(WorkItem workItem);

        /// <summary>
        /// Opens a pull request in Visual Studio
        /// </summary>
        /// <param name="pullRequest">Pull request to open</param>
        /// <returns>True if launched successfully</returns>
        bool OpenPullRequestInVisualStudio(PullRequest pullRequest);

        /// <summary>
        /// Opens a code review in Visual Studio
        /// </summary>
        /// <param name="codeReview">Code review to open</param>
        /// <returns>True if launched successfully</returns>
        bool OpenCodeReviewInVisualStudio(CodeReview codeReview);

        /// <summary>
        /// Opens a URL in the default browser
        /// </summary>
        /// <param name="url">URL to open</param>
        /// <returns>True if launched successfully</returns>
        bool OpenInBrowser(string url);

        /// <summary>
        /// Checks if Visual Studio is installed
        /// </summary>
        /// <returns>True if Visual Studio detected</returns>
        bool IsVisualStudioInstalled();
    }
}
```

---

### ICredentialStore

Service interface for secure credential management.

```csharp
namespace TfsViewer.Core.Services
{
    /// <summary>
    /// Service for storing and retrieving TFS credentials securely
    /// </summary>
    public interface ICredentialStore
    {
        /// <summary>
        /// Stores TFS credentials in Windows Credential Manager
        /// </summary>
        /// <param name="serverUrl">TFS server URL</param>
        /// <param name="credentials">Credentials to store</param>
        void StoreCredentials(string serverUrl, TfsCredentials credentials);

        /// <summary>
        /// Retrieves TFS credentials from Windows Credential Manager
        /// </summary>
        /// <param name="serverUrl">TFS server URL</param>
        /// <returns>Stored credentials or null if not found</returns>
        TfsCredentials? RetrieveCredentials(string serverUrl);

        /// <summary>
        /// Deletes stored credentials
        /// </summary>
        /// <param name="serverUrl">TFS server URL</param>
        void DeleteCredentials(string serverUrl);

        /// <summary>
        /// Checks if credentials exist for server
        /// </summary>
        /// <param name="serverUrl">TFS server URL</param>
        /// <returns>True if credentials stored</returns>
        bool HasCredentials(string serverUrl);
    }
}
```

---

## Data Transfer Objects (DTOs)

### TfsCredentials

```csharp
namespace TfsViewer.Core.Models
{
    /// <summary>
    /// Credentials for TFS authentication
    /// </summary>
    public class TfsCredentials
    {
        public string? Username { get; set; }
        public string? PersonalAccessToken { get; set; }
        public AuthenticationType Type { get; set; }
    }

    public enum AuthenticationType
    {
        PersonalAccessToken,
        WindowsAuthentication
    }
}
```

---

### ConnectionResult

```csharp
namespace TfsViewer.Core.Models
{
    /// <summary>
    /// Result of TFS connection attempt
    /// </summary>
    public class ConnectionResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserDisplayName { get; set; }
        public DateTime? ConnectedAt { get; set; }
    }
}
```

---

## TFS REST API Endpoints

### 1. Get Assigned Work Items

**Endpoint**: `POST {serverUrl}/_apis/wit/wiql?api-version=6.0`

**Request Body**:
```json
{
  "query": "SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State], [System.AssignedTo], [System.ChangedDate] FROM WorkItems WHERE [System.AssignedTo] = @Me ORDER BY [System.ChangedDate] DESC"
}
```

**Response**:
```json
{
  "workItems": [
    { "id": 12345, "url": "https://tfs.company.com/_apis/wit/workItems/12345" }
  ]
}
```

**Then fetch details**: `GET {serverUrl}/_apis/wit/workitems?ids={id1,id2,id3}&api-version=6.0`

**Response**:
```json
{
  "value": [
    {
      "id": 12345,
      "fields": {
        "System.Title": "Fix login bug",
        "System.WorkItemType": "Bug",
        "System.State": "Active",
        "System.AssignedTo": {
          "displayName": "Current User",
          "uniqueName": "user@company.com"
        },
        "System.ChangedDate": "2025-11-20T10:30:00Z"
      },
      "_links": {
        "html": { "href": "https://tfs.company.com/..." }
      }
    }
  ]
}
```

---

### 2. Get Pull Requests for Review

**Endpoint**: `GET {serverUrl}/_apis/git/pullrequests?searchCriteria.reviewerId={userId}&searchCriteria.status=active&api-version=6.0`

**Response**:
```json
{
  "value": [
    {
      "pullRequestId": 456,
      "title": "Implement user authentication",
      "createdBy": {
        "displayName": "John Doe",
        "uniqueName": "john.doe@company.com"
      },
      "creationDate": "2025-11-22T14:00:00Z",
      "status": "active",
      "sourceRefName": "refs/heads/feature/auth",
      "targetRefName": "refs/heads/main",
      "repository": {
        "name": "MyRepo",
        "project": { "name": "MyProject" }
      }
    }
  ]
}
```

---

### 3. Get Code Reviews (TFVC)

**Endpoint**: `GET {serverUrl}/_apis/tfvc/codeReviews?assignedTo={userId}&api-version=6.0`

**Response**:
```json
{
  "value": [
    {
      "codeReviewId": 789,
      "title": "Review database schema changes",
      "requestedBy": {
        "displayName": "Alice Wang",
        "uniqueName": "alice.wang@company.com"
      },
      "creationDate": "2025-11-23T09:15:00Z",
      "status": "pending",
      "project": { "name": "MyProject" }
    }
  ]
}
```

---

## Error Responses

**Authentication Failure** (401):
```json
{
  "message": "The requested resource requires user authentication.",
  "typeKey": "UnauthorizedException"
}
```

**Connection Error** (Network):
```json
{
  "error": "Unable to connect to TFS server",
  "details": "Connection timeout after 30 seconds"
}
```

**Rate Limit** (429):
```json
{
  "message": "Rate limit exceeded. Try again later.",
  "retryAfter": 60
}
```

---

## Client-Side Contracts

### View Models (for UI binding)

```csharp
namespace TfsViewer.App.ViewModels
{
    /// <summary>
    /// ViewModel for main window with tabs
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<WorkItemViewModel> WorkItems { get; }
        public ObservableCollection<PullRequestViewModel> PullRequests { get; }
        public ObservableCollection<CodeReviewViewModel> CodeReviews { get; }
        
        public int WorkItemCount => WorkItems.Count;
        public int PullRequestCount => PullRequests.Count;
        public int CodeReviewCount => CodeReviews.Count;
        
        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand MinimizeToTrayCommand { get; }
        public IRelayCommand ExitCommand { get; }
    }

    /// <summary>
    /// ViewModel for individual work item in list
    /// </summary>
    public class WorkItemViewModel : ObservableObject
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Type { get; init; }
        public string State { get; init; }
        public DateTime AssignedDate { get; init; }
        public string TfsWebUrl { get; init; }
        
        public IRelayCommand OpenInBrowserCommand { get; }
        public IRelayCommand OpenInVisualStudioCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
    }

    // Similar for PullRequestViewModel and CodeReviewViewModel
}
```

---

## Configuration Schema

**appsettings.json**:

```json
{
  "TfsViewer": {
    "RefreshInterval": 60,
    "CacheTtl": {
      "WorkItems": 300,
      "PullRequests": 120,
      "CodeReviews": 300
    },
    "Performance": {
      "MaxItemsPerTab": 500,
      "EnableDiskCache": true,
      "DiskCachePath": "%LOCALAPPDATA%\\TfsViewer\\cache"
    },
    "UI": {
      "Theme": "Light",
      "AccentColor": "#0078D4",
      "MinimizeToTray": true,
      "ShowBalloonNotifications": true
    }
  }
}
```

---

## Protocol Handlers

### Visual Studio URLs

**Work Item**:
```
vstfs:///WorkItemTracking/WorkItem/{id}?url={encodedTfsUrl}
```

**Pull Request** (via TFS Team Explorer):
```
vstfs:///CodeReview/CodeReview/{reviewId}?url={encodedTfsUrl}
```

**Git Pull Request** (requires Git Tools for VS):
```
vs://team/pullrequest/{repoId}/{prId}?url={encodedTfsUrl}
```

**Code Review**:
```
vstfs:///VersionControl/Changeset/{changesetId}?url={encodedTfsUrl}
```

### Browser URLs

**Work Item**:
```
{serverUrl}/{project}/_workitems/edit/{id}
```

**Pull Request**:
```
{serverUrl}/{project}/_git/{repo}/pullrequest/{prId}
```

**Code Review**:
```
{serverUrl}/{project}/_versionControl/codeReview/{reviewId}
```

---

## Authentication Flows

### Flow 1: Personal Access Token (PAT)

```
1. User enters Server URL + PAT in settings
2. App calls ConnectAsync with PAT
3. TfsService creates VssConnection with VssBasicCredential
4. Service validates by calling GET _apis/connectionData
5. On success, store PAT in Windows Credential Manager
6. Return ConnectionResult with user display name
```

### Flow 2: Windows Authentication

```
1. User enters Server URL, selects Windows Auth
2. App calls ConnectAsync with null credentials
3. TfsService creates VssConnection with VssClientCredentials (default)
4. Windows handles authentication (NTLM/Kerberos)
5. Service validates by calling GET _apis/connectionData
6. On success, store server URL only (no credentials needed)
7. Return ConnectionResult with user display name
```

---

## Data Refresh Strategy

### On Application Startup

```
1. Load credentials from Windows Credential Manager
2. Load cached data from disk (if exists) → display in UI (instant)
3. Background: Connect to TFS in parallel
4. Background: Fetch fresh data (WorkItems + PRs + Reviews in parallel)
5. Update cache and UI when fresh data arrives
6. Total time: ~1-2 seconds (cached display) + 3-4 seconds (fresh data)
```

### Manual Refresh

```
1. User clicks Refresh button
2. Disable refresh button, show loading indicator
3. Fetch all data types in parallel (3 API calls)
4. Update cache and UI
5. Re-enable refresh button, hide loading indicator
6. Total time: ~3-5 seconds
```

### Background Auto-Refresh (Optional)

```
1. Timer triggers every 60 seconds (configurable)
2. If app is active (not minimized), fetch fresh data silently
3. Update UI if changes detected (e.g., new PR assigned)
4. Optional: Show balloon notification for new items
```

---

## Error Handling Contracts

### ServiceException

```csharp
namespace TfsViewer.Core.Exceptions
{
    public class TfsServiceException : Exception
    {
        public ErrorCode Code { get; }
        public string UserMessage { get; }
        
        public TfsServiceException(ErrorCode code, string userMessage, Exception? innerException = null)
            : base(userMessage, innerException)
        {
            Code = code;
            UserMessage = userMessage;
        }
    }

    public enum ErrorCode
    {
        ConnectionFailed,
        AuthenticationFailed,
        ServerUnreachable,
        InvalidResponse,
        RateLimitExceeded,
        Timeout,
        Unknown
    }
}
```

### Error Messages (User-Facing)

| Error Code | User Message | Action |
|------------|--------------|--------|
| ConnectionFailed | "Could not connect to TFS server. Check your network connection." | Retry button |
| AuthenticationFailed | "Login failed. Please check your credentials." | Re-enter credentials |
| ServerUnreachable | "TFS server is not responding. Please try again later." | Retry button |
| RateLimitExceeded | "Too many requests. Please wait a moment before refreshing." | Wait + retry |
| Timeout | "Request timed out. The server may be slow." | Retry button |

---

## Performance Contracts

### Service Level Objectives (SLOs)

| Operation | Target | Measurement |
|-----------|--------|-------------|
| ConnectAsync | <3s | Time from call to ConnectionResult |
| GetAssignedWorkItemsAsync | <4s | Time to fetch + parse up to 500 items |
| GetPullRequestsAsync | <3s | Time to fetch + parse up to 100 PRs |
| GetCodeReviewsAsync | <3s | Time to fetch + parse up to 50 reviews |
| Cache.Get | <10ms | In-memory lookup |
| UI Refresh (from cache) | <50ms | Time to update ObservableCollection |

### Resource Limits

| Resource | Limit | Enforcement |
|----------|-------|-------------|
| Memory | 100MB | Monitor via PerformanceCounter, warn at 90MB |
| Network requests | 10/min | Throttle with rate limiter |
| Cached items | 1500 total | Evict oldest if exceeded |
| Cache disk size | 10MB | Clear old cache files |

---

## API Version Compatibility

**Supported TFS Versions**:
- TFS 2015 Update 2+
- TFS 2017+
- TFS 2018+
- Azure DevOps Server 2019+
- Azure DevOps Server 2020+

**API Version**: Use `api-version=6.0` for REST calls (compatible with all versions above)

**Compatibility Testing**: Test against at least TFS 2017 and Azure DevOps Server 2020

---

## Security Considerations

1. **Credentials**: Stored encrypted in Windows Credential Manager
2. **HTTPS Only**: Reject HTTP connections, require HTTPS
3. **No Logging of Secrets**: Never log PATs or passwords
4. **Certificate Validation**: Validate SSL certificates (allow user override for self-signed)
5. **Read-Only**: No write operations, minimize permissions required (read work items, read code)

---

## Extension Points (Future)

Not in MVP, but contracts designed to support:
- Custom queries/filters
- Export functionality
- Notifications service
- Settings sync across machines
