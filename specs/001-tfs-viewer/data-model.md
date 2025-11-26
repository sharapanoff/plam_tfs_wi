# Data Model: TFS Read-Only Viewer

**Feature**: 001-tfs-viewer  
**Date**: 2025-11-25  
**Source**: Extracted from spec.md requirements

## Overview

This document defines the data entities for the TFS Read-Only Viewer application. All entities are read-only and sourced from TFS Server via REST API.

---

## Core Entities

### 1. WorkItem

Represents a TFS work item (bug, task, user story, etc.) assigned to the user.

**Fields**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| Id | int | Yes | Unique TFS work item ID | > 0 |
| Title | string | Yes | Brief description of work item | Max 255 chars, not empty |
| Type | string | Yes | Work item type (Bug, Task, User Story, etc.) | Enum or free text from TFS |
| State | string | Yes | Current state (New, Active, Resolved, Closed) | Not empty |
| AssignedDate | DateTime | Yes | When item was assigned to current user | Valid DateTime |
| TfsWebUrl | string | Yes | URL to view in browser | Valid URL format |
| VisualStudioUrl | string | Yes | Protocol URL for Visual Studio | vstfs:// protocol |
| ProjectName | string | No | TFS project name | Max 255 chars |
| Priority | int | No | Work item priority | 1-4 typical |
| AreaPath | string | No | TFS area path | Max 255 chars |

**State Transitions**: N/A (read-only, no mutations)

**Relationships**:
- One WorkItem may have child WorkItems (hierarchical)
- Many WorkItems belong to one Project

**Indexes/Keys**:
- Primary Key: `Id` (unique identifier)
- Index on `AssignedDate` for sorting

**Example**:
```json
{
  "id": 12345,
  "title": "Fix login bug in authentication module",
  "type": "Bug",
  "state": "Active",
  "assignedDate": "2025-11-20T10:30:00Z",
  "tfsWebUrl": "https://tfs.company.com/DefaultCollection/MyProject/_workitems/edit/12345",
  "visualStudioUrl": "vstfs:///WorkItemTracking/WorkItem/12345?url=https://tfs.company.com",
  "projectName": "MyProject",
  "priority": 1,
  "areaPath": "MyProject\\Authentication"
}
```

---

### 2. PullRequest

Represents a code review pull request where the user is a reviewer.

**Fields**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| Id | int | Yes | Unique pull request ID | > 0 |
| Title | string | Yes | Description of changes | Max 500 chars, not empty |
| Author | string | Yes | User who created the PR | Not empty |
| AuthorDisplayName | string | No | Friendly display name of author | Max 255 chars |
| CreationDate | DateTime | Yes | When PR was created | Valid DateTime |
| Status | string | Yes | PR status (Active, Completed, Abandoned) | Enum: Active/Completed/Abandoned |
| SourceBranch | string | Yes | Source branch name | Not empty |
| TargetBranch | string | Yes | Target branch name | Not empty |
| ReviewerIds | string[] | No | List of reviewer user IDs | Array of strings |
| TfsWebUrl | string | Yes | URL to view in browser | Valid URL format |
| VisualStudioUrl | string | Yes | Protocol URL for Visual Studio | vsdiffmerge:// protocol |
| ProjectName | string | Yes | TFS project name | Max 255 chars |
| RepositoryName | string | Yes | Git repository name | Max 255 chars |

**State Transitions**: N/A (read-only)

**Relationships**:
- One PullRequest belongs to one Repository
- One PullRequest has many Reviewers (users)
- Many PullRequests belong to one Project

**Indexes/Keys**:
- Primary Key: `Id`
- Index on `CreationDate` for sorting
- Index on `Status` for filtering

**Example**:
```json
{
  "id": 456,
  "title": "Implement user authentication feature",
  "author": "john.doe@company.com",
  "authorDisplayName": "John Doe",
  "creationDate": "2025-11-22T14:00:00Z",
  "status": "Active",
  "sourceBranch": "feature/auth-module",
  "targetBranch": "main",
  "reviewerIds": ["jane.smith@company.com", "bob.jones@company.com"],
  "tfsWebUrl": "https://tfs.company.com/DefaultCollection/MyProject/_git/MyRepo/pullrequest/456",
  "visualStudioUrl": "vsdiffmerge:///pullrequest/456?url=https://tfs.company.com",
  "projectName": "MyProject",
  "repositoryName": "MyRepo"
}
```

---

### 3. CodeReview

Represents a formal code review (TFVC-based) assigned to the user.

**Fields**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| Id | int | Yes | Unique code review ID | > 0 |
| Title | string | Yes | Review description | Max 500 chars, not empty |
| Requester | string | Yes | User who requested review | Not empty |
| RequesterDisplayName | string | No | Friendly display name of requester | Max 255 chars |
| CreationDate | DateTime | Yes | When review was created | Valid DateTime |
| Status | string | Yes | Review status (Pending, Completed) | Enum: Pending/Completed |
| TfsWebUrl | string | Yes | URL to view in browser | Valid URL format |
| VisualStudioUrl | string | Yes | Protocol URL for Visual Studio | vstfs:// protocol |
| ProjectName | string | Yes | TFS project name | Max 255 chars |
| ChangesetId | int | No | Associated changeset ID | > 0 if present |

**State Transitions**: N/A (read-only)

**Relationships**:
- One CodeReview belongs to one Project
- One CodeReview may reference one Changeset
- One CodeReview has one assigned Reviewer (current user)

**Indexes/Keys**:
- Primary Key: `Id`
- Index on `CreationDate` for sorting
- Index on `Status` for filtering

**Example**:
```json
{
  "id": 789,
  "title": "Review database schema changes",
  "requester": "alice.wang@company.com",
  "requesterDisplayName": "Alice Wang",
  "creationDate": "2025-11-23T09:15:00Z",
  "status": "Pending",
  "tfsWebUrl": "https://tfs.company.com/DefaultCollection/MyProject/_versionControl/codeReview/789",
  "visualStudioUrl": "vstfs:///CodeReview/CodeReview/789?url=https://tfs.company.com",
  "projectName": "MyProject",
  "changesetId": 54321
}
```

---

## Supporting Entities

### 4. TfsConnection

Represents the connection configuration to TFS Server.

**Fields**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| ServerUrl | string | Yes | TFS server base URL | Valid HTTPS URL |
| Username | string | No | Username (if not using PAT) | Max 255 chars |
| PersonalAccessToken | string | No | Personal Access Token | Encrypted, not displayed |
| AuthenticationType | string | Yes | Auth type (PAT, Windows) | Enum: PAT/Windows |
| ConnectionStatus | string | Yes | Current status | Enum: Connected/Disconnected/Error |
| LastRefreshTime | DateTime | No | Last successful data refresh | Valid DateTime |
| IsConnected | bool | Yes | Connection state | true/false |

**State Transitions**:
- Disconnected → Connected (on successful auth)
- Connected → Disconnected (on logout or error)
- Connected → Error (on connection failure)
- Error → Connected (on retry success)

**Validation Rules**:
- ServerUrl must start with `https://`
- Either PersonalAccessToken or Username must be provided
- If AuthenticationType is PAT, PersonalAccessToken is required
- If AuthenticationType is Windows, Username may be optional (use current Windows user)

**Example**:
```json
{
  "serverUrl": "https://tfs.company.com/DefaultCollection",
  "username": null,
  "personalAccessToken": "[ENCRYPTED]",
  "authenticationType": "PAT",
  "connectionStatus": "Connected",
  "lastRefreshTime": "2025-11-25T15:00:00Z",
  "isConnected": true
}
```

---

### 5. CachedItem (Internal)

Represents cached data for offline/fast loading. Not exposed to UI directly.

**Fields**:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Key | string | Yes | Cache key (e.g., "workitems_user123") |
| Data | string | Yes | Serialized JSON of cached entities |
| ExpirationTime | DateTime | Yes | When cache expires |
| ItemType | string | Yes | Type of cached item (WorkItem, PullRequest, CodeReview) |

**Storage**: In-memory (MemoryCache) with optional disk persistence (JSON files)

**TTL (Time To Live)**:
- WorkItems: 5 minutes
- PullRequests: 2 minutes (more volatile)
- CodeReviews: 5 minutes

---

## Data Flow

```
TFS Server (REST API)
    ↓
TfsService (fetch data via Microsoft.TeamFoundationServer.Client)
    ↓
CacheService (store in MemoryCache, optional disk cache)
    ↓
ViewModels (expose to UI via ObservableCollections)
    ↓
Views (display in DataGrids/ListViews with virtualization)
```

**Refresh Strategy**:
1. On app startup: Load from disk cache (if exists) → display immediately
2. Background: Fetch fresh data from TFS → update cache → update UI
3. Manual refresh: User clicks refresh → fetch from TFS → update UI
4. Auto-refresh: Every 60 seconds in background (configurable)

---

## Entity Counts & Performance

**Expected Scale**:
- WorkItems: 50-200 per user (max 500)
- PullRequests: 5-20 per user (max 100)
- CodeReviews: 2-10 per user (max 50)

**Memory Estimates** (per item):
- WorkItem: ~500 bytes (including strings)
- PullRequest: ~800 bytes
- CodeReview: ~600 bytes

**Total Memory** (max load):
- 500 WorkItems × 500 bytes = 250 KB
- 100 PullRequests × 800 bytes = 80 KB
- 50 CodeReviews × 600 bytes = 30 KB
- **Total**: ~360 KB data + ~30 MB UI overhead + ~20 MB framework = **~50-60 MB**

---

## Validation Summary

All entities are **read-only** - no create, update, or delete operations.

**Cross-Entity Validation**:
- All dates must be in UTC
- All URLs must be valid and accessible
- All IDs must be positive integers
- All required string fields must be non-empty and trimmed

**Error Handling**:
- Missing required fields → log error, skip item, show notification
- Invalid URL format → log warning, disable "Open" buttons for that item
- API errors → cache previous data, show error banner, retry with exponential backoff

---

## Future Considerations

**Not in MVP** (for future versions):
- Filtering/search within tabs
- Sorting by multiple columns
- Custom grouping (by project, by state)
- Export to CSV/Excel
- Desktop notifications for new items
- Detailed diff view (would require write permissions or complex rendering)
