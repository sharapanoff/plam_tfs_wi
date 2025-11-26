# Tasks: TFS Read-Only Viewer Application

**Input**: Design documents from `/specs/001-tfs-viewer/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Not explicitly requested in feature specification - test tasks are EXCLUDED per specification guidelines.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md structure:
- Main application: `src/TfsViewer.App/`
- Core library: `src/TfsViewer.Core/`
- Tests: `tests/TfsViewer.App.Tests/`, `tests/TfsViewer.Core.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution file TfsViewer.sln at repository root
- [X] T002 Create TfsViewer.App project in src/TfsViewer.App/TfsViewer.App.csproj (WPF, .NET 6/8)
- [X] T003 Create TfsViewer.Core project in src/TfsViewer.Core/TfsViewer.Core.csproj (Class Library, .NET 6/8)
- [X] T004 Add project reference from TfsViewer.App to TfsViewer.Core
- [X] T005 [P] Install MaterialDesignThemes NuGet package (4.9+) in TfsViewer.App
- [X] T006 [P] Install MaterialDesignColors NuGet package (2.1+) in TfsViewer.App
- [X] T007 [P] Install CommunityToolkit.Mvvm NuGet package (8.2+) in TfsViewer.App
- [X] T008 [P] Install Hardcodet.NotifyIcon.Wpf NuGet package (1.1+) in TfsViewer.App
- [X] T009 [P] Install Microsoft.TeamFoundationServer.Client NuGet package in TfsViewer.Core
- [X] T010 [P] Install CredentialManagement NuGet package (1.0.2) in TfsViewer.Core
- [X] T011 [P] Install System.Runtime.Caching NuGet package in TfsViewer.Core
- [X] T012 [P] Create Resources directory in src/TfsViewer.App/Resources/
- [X] T013 [P] Create folder structure for Views in src/TfsViewer.App/Views/
- [X] T014 [P] Create folder structure for ViewModels in src/TfsViewer.App/ViewModels/
- [X] T015 [P] Create folder structure for Models in src/TfsViewer.Core/Models/
- [X] T016 [P] Create folder structure for Services in src/TfsViewer.Core/Services/
- [X] T017 [P] Create folder structure for Contracts in src/TfsViewer.Core/Contracts/
- [X] T018 [P] Create folder structure for Api in src/TfsViewer.Core/Api/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T019 [P] Create WorkItem model in src/TfsViewer.Core/Models/WorkItem.cs
- [X] T020 [P] Create PullRequest model in src/TfsViewer.Core/Models/PullRequest.cs
- [X] T021 [P] Create CodeReview model in src/TfsViewer.Core/Models/CodeReview.cs
- [X] T022 [P] Create TfsConnection model in src/TfsViewer.Core/Models/TfsConnection.cs
- [X] T023 [P] Create TfsCredentials model in src/TfsViewer.Core/Models/TfsCredentials.cs
- [X] T024 [P] Create ConnectionResult model in src/TfsViewer.Core/Models/ConnectionResult.cs
- [X] T025 [P] Create ITfsService interface in src/TfsViewer.Core/Contracts/ITfsService.cs
- [X] T026 [P] Create ICacheService interface in src/TfsViewer.Core/Contracts/ICacheService.cs
- [X] T027 [P] Create ICredentialStore interface in src/TfsViewer.Core/Contracts/ICredentialStore.cs
- [X] T028 [P] Create ILauncherService interface in src/TfsViewer.App/Services/ILauncherService.cs
- [X] T029 Create CredentialStore service in src/TfsViewer.Core/Infrastructure/CredentialStore.cs implementing ICredentialStore
- [X] T030 Create CacheService in src/TfsViewer.Core/Services/CacheService.cs implementing ICacheService
- [X] T031 Create TfsApiClient in src/TfsViewer.Core/Api/TfsApiClient.cs for TFS REST API communication
- [X] T032 Create Configuration class in src/TfsViewer.App/Infrastructure/Configuration.cs for app settings
- [X] T033 Create appsettings.json in src/TfsViewer.App/ with configuration schema
- [X] T034 Setup dependency injection container in src/TfsViewer.App/App.xaml.cs
- [X] T035 [P] Create Material Design resource dictionary in src/TfsViewer.App/Resources/Styles.xaml
- [X] T036 [P] Create application icons in src/TfsViewer.App/Resources/Icons/
- [X] T037 Configure logging infrastructure using ILogger in src/TfsViewer.Core/
- [X] T038 Create TfsServiceException class in src/TfsViewer.Core/Exceptions/TfsServiceException.cs
- [X] T038a [P] Install Polly NuGet package (8.x) in TfsViewer.Core for retry policy
- [X] T038b [P] Install Serilog NuGet package (3.x) in TfsViewer.Core for structured logging
- [X] T038c [P] Install Serilog.Sinks.File NuGet package in TfsViewer.Core for file logging

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View Assigned Work Items (Priority: P1) 🎯 MVP

**Goal**: Display all work items assigned to the user with basic information (ID, title, status, type) and allow opening them in browser or Visual Studio

**Independent Test**: Connect to a TFS server with at least one work item assigned to the user and verify the work item displays with its basic information. Click "Open in Browser" and "Open in Visual Studio" buttons to verify they launch correctly.

### Implementation for User Story 1

- [X] T039 [US1] Create TfsService in src/TfsViewer.Core/Services/TfsService.cs implementing ITfsService
- [X] T040 [US1] Implement ConnectAsync method in TfsService for TFS connection and authentication
- [X] T041 [US1] Implement GetAssignedWorkItemsAsync method in TfsService using WIQL query
- [X] T042 [US1] Implement TestConnectionAsync method in TfsService
- [X] T043 [US1] Implement Disconnect method in TfsService
- [X] T044 [P] [US1] Create WorkItemViewModel in src/TfsViewer.App/ViewModels/WorkItemViewModel.cs
- [X] T045 [P] [US1] Create WorkItemsTabViewModel in src/TfsViewer.App/ViewModels/WorkItemsTabViewModel.cs (renamed from StoriesTabViewModel)
- [X] T046 [US1] Implement LoadWorkItemsAsync command in WorkItemsTabViewModel
- [X] T047 [US1] Create LauncherService in src/TfsViewer.App/Services/LauncherService.cs implementing ILauncherService
- [X] T048 [US1] Implement OpenWorkItemInVisualStudio method in LauncherService
- [X] T049 [US1] Implement OpenInBrowser method in LauncherService
- [X] T050 [US1] Implement IsVisualStudioInstalled method in LauncherService
- [X] T051 [US1] Create SettingsWindow.xaml in src/TfsViewer.App/Views/SettingsWindow.xaml for TFS connection settings
- [X] T052 [US1] Create SettingsViewModel in src/TfsViewer.App/ViewModels/SettingsViewModel.cs
- [X] T053 [US1] Implement connection form with server URL and authentication fields in SettingsWindow.xaml
- [X] T054 [US1] Implement ConnectCommand in SettingsViewModel to save credentials and test connection
- [X] T055 [US1] Create MainWindow.xaml in src/TfsViewer.App/Views/MainWindow.xaml with TabControl
- [X] T056 [US1] Create MainViewModel in src/TfsViewer.App/ViewModels/MainViewModel.cs
- [X] T057 [US1] Add Work Items tab to MainWindow.xaml with DataGrid bound to WorkItems collection
- [X] T058 [US1] Create DataTemplate for WorkItem display in MainWindow.xaml (ID, Title, Type, State, Date)
- [X] T059 [US1] Add "Open in Browser" button to WorkItem row template with command binding
- [X] T060 [US1] Add "Open in Visual Studio" button to WorkItem row template with command binding
- [X] T061 [US1] Implement RefreshCommand in MainViewModel to reload work items
- [X] T062 [US1] Add work item count badge to Work Items tab header with binding
- [X] T063 [US1] Implement error handling for connection failures in WorkItemsTabViewModel
- [X] T064 [US1] Implement error handling for API failures in TfsService.GetAssignedWorkItemsAsync
- [X] T065 [US1] Add loading indicator to Work Items tab during data fetch
- [X] T066 [US1] Add empty state message when no work items assigned
- [X] T067 [US1] Register all services in dependency injection in App.xaml.cs
- [X] T068 [US1] Configure MainWindow as startup window in App.xaml.cs
- [X] T069 [US1] Implement credential loading on app startup in App.xaml.cs
- [X] T070 [US1] Show SettingsWindow if no credentials found on startup

- [X] T070a [US1] Create LoggingService in src/TfsViewer.Core/Services/LoggingService.cs with Serilog configuration (FR-030)
- [X] T070b [US1] Configure Serilog to write errors/warnings only to %LOCALAPPDATA%\TfsViewer\logs\app-.log with rolling daily files (FR-030)
- [X] T070c [US1] Integrate LoggingService into TfsService error handling for API failures (FR-030)

**Checkpoint**: At this point, User Story 1 should be fully functional - users can connect to TFS, view assigned work items, and open them in browser/VS

---

## Phase 4: User Story 4 - Refresh Data (Priority: P2)

**Goal**: Allow users to manually refresh data from TFS server to see the most current information

**Independent Test**: Modify work item data on TFS server (e.g., change state, reassign), then click Refresh in the application and verify updated information displays. Test with TFS server offline to verify error message appears.

### Implementation for User Story 4

- [X] T071 [US4] Implement cache integration in TfsService.GetAssignedWorkItemsAsync with 5-minute TTL
- [X] T072 [US4] Add cache invalidation on manual refresh in MainViewModel.RefreshCommand
- [X] T073 [US4] Add Refresh button to MainWindow.xaml toolbar with command binding
- [X] T074 [US4] Implement RefreshAllCommand in MainViewModel to refresh all tabs
- [X] T075 [US4] Add loading spinner to Refresh button during refresh operation
- [X] T076 [US4] Disable Refresh button during active refresh to prevent multiple simultaneous calls
- [X] T077 [US4] Implement error handling for TFS server unreachable in RefreshCommand
- [X] T078 [US4] Show user-friendly error message dialog when refresh fails
- [X] T079 [US4] Add last refresh timestamp display to MainWindow.xaml status bar
- [X] T080 [US4] Update last refresh timestamp after successful refresh operation
 - [X] T081 [US4] Create Polly retry policy in src/TfsViewer.Core/Infrastructure/RetryPolicy.cs with 3 retries and exponential backoff (FR-028)
 - [X] T081a [US4] Apply Polly retry policy to TfsService.GetAssignedWorkItemsAsync with VssServiceException and HttpRequestException handling (FR-028)
 - [X] T081b [US4] Log retry attempts with LoggingService including attempt number and delay duration (FR-028, FR-030)
- [ ] T082 [US4] Implement timeout handling (30 seconds) for TFS API calls

**Checkpoint**: At this point, User Stories 1 AND 4 should both work - users can view work items and refresh them on demand

---

## Phase 5: User Story 2 - View Pull Requests (Priority: P2)

**Goal**: Display all pull requests where the user is a reviewer with relevant details (ID, title, author, creation date, status) and allow opening them in browser or Visual Studio

**Independent Test**: Create a pull request assigned to the user for review on TFS server and verify it appears in the Pull Requests tab with all details. Click "Open in Browser" and "Open in Visual Studio" to verify they launch correctly.

### Implementation for User Story 2

 - [X] T083 [P] [US2] Create PullRequestViewModel in src/TfsViewer.App/ViewModels/PullRequestViewModel.cs
 - [X] T084 [P] [US2] Create PullRequestTabViewModel in src/TfsViewer.App/ViewModels/PullRequestTabViewModel.cs
 - [X] T085 [US2] Implement GetPullRequestsAsync method in TfsService using Git Pull Requests API
 - [X] T086 [US2] Implement LoadPullRequestsAsync command in PullRequestTabViewModel
 - [ ] T087 [US2] Implement cache integration for pull requests with 2-minute TTL in TfsService
 - [X] T088 [US2] Implement OpenPullRequestInVisualStudio method in LauncherService
 - [X] T089 [US2] Add Pull Requests tab to MainWindow.xaml with DataGrid
 - [X] T090 [US2] Create DataTemplate for PullRequest display (ID, Title, Author, Date, Status)
 - [X] T091 [US2] Add "Open in Browser" button to PullRequest row template
 - [X] T092 [US2] Add "Open in Visual Studio" button to PullRequest row template
 - [X] T093 [US2] Add pull request count badge to tab header with binding
 - [X] T094 [US2] Add PullRequestTabViewModel to MainViewModel composition
 - [X] T095 [US2] Integrate pull requests refresh into MainViewModel.RefreshAllCommand
 - [X] T096 [US2] Add loading indicator to Pull Requests tab during data fetch
 - [X] T097 [US2] Add empty state message when no pull requests assigned
 - [X] T098 [US2] Implement error handling for pull requests API failures
 - [X] T099 [US2] Register PullRequestTabViewModel in dependency injection

**Checkpoint**: At this point, User Stories 1, 2, and 4 should all work independently - users can view work items and pull requests, and refresh both

---

## Phase 6: User Story 3 - View Code Reviews (Priority: P3)

**Goal**: Display all code reviews assigned to the user with review details (ID, title, requester, creation date, status) and allow opening them in browser or Visual Studio

**Independent Test**: Assign a code review to the user on TFS server and verify it appears in the Code Reviews tab with all details. Click "Open in Browser" and "Open in Visual Studio" to verify they launch correctly.

### Implementation for User Story 3

 - [X] T100 [P] [US3] Create CodeReviewViewModel in src/TfsViewer.App/ViewModels/CodeReviewViewModel.cs
 - [X] T101 [P] [US3] Create CodeReviewTabViewModel in src/TfsViewer.App/ViewModels/CodeReviewTabViewModel.cs
 - [X] T102 [US3] Implement GetCodeReviewsAsync method in TfsService using TFVC Code Reviews API
 - [X] T103 [US3] Implement LoadCodeReviewsAsync command in CodeReviewTabViewModel
 - [ ] T104 [US3] Implement cache integration for code reviews with 5-minute TTL in TfsService
 - [X] T105 [US3] Implement OpenCodeReviewInVisualStudio method in LauncherService
 - [X] T106 [US3] Add Code Reviews tab to MainWindow.xaml with DataGrid
 - [X] T107 [US3] Create DataTemplate for CodeReview display (ID, Title, Requester, Date, Status)
 - [X] T108 [US3] Add "Open in Browser" button to CodeReview row template
 - [X] T109 [US3] Add "Open in Visual Studio" button to CodeReview row template
 - [X] T110 [US3] Add code review count badge to tab header with binding
 - [X] T111 [US3] Add CodeReviewTabViewModel to MainViewModel composition
 - [X] T112 [US3] Integrate code reviews refresh into MainViewModel.RefreshAllCommand
 - [X] T113 [US3] Add loading indicator to Code Reviews tab during data fetch
 - [X] T114 [US3] Add empty state message when no code reviews assigned
 - [X] T115 [US3] Implement error handling for code reviews API failures
 - [X] T116 [US3] Register CodeReviewTabViewModel in dependency injection

**Checkpoint**: All user stories should now be independently functional - users can view work items, pull requests, and code reviews, and refresh all data

---

## Phase 7: (Removed) System Tray Integration

Removed as out-of-scope (no specification requirement). Tasks T117–T126 deferred.

---

## Phase 8: Performance Optimization

**Purpose**: Ensure application meets performance targets (<2s load, <5s refresh, <100MB memory)

- [ ] T127 Implement UI virtualization with VirtualizingStackPanel in all DataGrids
- [ ] T128 Configure RecyclingMode="Recycling" on all ListViews for memory efficiency
- [ ] T129 Implement parallel data fetching for work items, PRs, and reviews in MainViewModel
- [ ] T130 Add CancellationToken support to all async service methods
- [ ] T131 Implement CancellationTokenSource in tab ViewModels for fast tab switching
- [ ] T132 Optimize DataTemplates to use minimal controls and freeze graphics objects
- [ ] T134 Add cache warming on app startup (load from memory cache, then refresh in background)
- [ ] T137 Implement lazy loading for detail views (only load on "View More" click)

Removed for scope focus: T133 disk cache, T135 LRU eviction, T136 performance counters, T138 rate limiting.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T139 [P] Add input validation to SettingsWindow for server URL format
- [ ] T140 [P] Add visual feedback for successful connection in SettingsWindow
- [ ] T142 [P] Add keyboard shortcuts (F5 for refresh, Ctrl+S for settings)
- [ ] T145 [P] Create application icon and set in TfsViewer.App project properties
- [ ] T146 Add "About" dialog with version information and credits
- [ ] T147 Implement graceful degradation when Visual Studio not installed (hide VS buttons)
- [ ] T148 Add tooltip documentation to all buttons and interactive elements
- [ ] T149 Implement accessibility features (keyboard navigation, screen reader support) (optional if later specified)
- [ ] T150 Add confirmation dialog for Exit command
- [ ] T151 Create README.md with quick start instructions in repository root
- [ ] T152 Update quickstart.md with actual implementation details
- [ ] T153 Add error logging to file for debugging (in %LOCALAPPDATA%\TfsViewer\logs\)
- [ ] T156 Run final performance profiling and optimize hot paths
- [ ] T157 Verify all FR requirements from spec.md are implemented
- [ ] T158 Validate application against success criteria from spec.md

Removed out-of-scope polish: T141 Remember me, T143 Theme selection, T144 Accent color, T154 Auto-update, T155 Telemetry.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion - MVP target
- **User Story 4 (Phase 4)**: Depends on US1 completion (extends refresh functionality)
- **User Story 2 (Phase 5)**: Depends on Foundational completion - Can run parallel to US3/US4
- **User Story 3 (Phase 6)**: Depends on Foundational completion - Can run parallel to US2/US4
- **System Tray (Phase 7)**: Depends on US1 completion (requires MainWindow)
- **Performance (Phase 8)**: Depends on all user stories being implemented
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories - **MVP TARGET**
- **User Story 4 (P2)**: Requires US1 complete (extends work items with refresh) - Can be implemented before US2/US3
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent from US1/US3/US4, integrates into refresh
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent from US1/US2/US4, integrates into refresh

### Within Each User Story

- Models before services (T019-T024 before T039)
- Services before ViewModels (T039-T043 before T044-T046)
- ViewModels before Views (T044-T046 before T055-T060)
- Core implementation before integration (each story complete before cross-story integration)
- Infrastructure before features (dependency injection setup before service usage)

### Parallel Opportunities

- **Setup Phase**: All NuGet installations (T005-T011) can run in parallel
- **Setup Phase**: All folder structure creation (T012-T018) can run in parallel
- **Foundational Phase**: All model creation (T019-T024) can run in parallel
- **Foundational Phase**: All interface creation (T025-T028) can run in parallel
- **Foundational Phase**: Resource files (T035-T036) can run in parallel
- **User Story 1**: WorkItemViewModel and WorkItemsTabViewModel (T044-T045) can be created in parallel
- **User Story 2**: PullRequestViewModel and PullRequestTabViewModel (T083-T084) can be created in parallel
- **User Story 3**: CodeReviewViewModel and CodeReviewTabViewModel (T100-T101) can be created in parallel
- **After Foundational**: US2 and US3 can be worked on in parallel (independent implementations)
- **Polish Phase**: Documentation tasks (T139-T145, T151-T152) can run in parallel

---

## Parallel Example: User Story 1 (Updated naming)

```bash
# Create ViewModels in parallel:
Task T044: "Create WorkItemViewModel in src/TfsViewer.App/ViewModels/WorkItemViewModel.cs"
Task T045: "Create WorkItemsTabViewModel in src/TfsViewer.App/ViewModels/WorkItemsTabViewModel.cs"

# After TfsService complete, implement launcher methods in parallel:
Task T048: "Implement OpenWorkItemInVisualStudio method in LauncherService"
Task T049: "Implement OpenInBrowser method in LauncherService"
Task T050: "Implement IsVisualStudioInstalled method in LauncherService"
```

---

## Parallel Example: Multiple User Stories

```bash
# After Foundational Phase complete, assign to different developers:
Developer A: User Story 2 (Pull Requests) - Tasks T083-T099
Developer B: User Story 3 (Code Reviews) - Tasks T100-T116

# These can proceed completely independently
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 4)

1. Complete Phase 1: Setup (T001-T018)
2. Complete Phase 2: Foundational (T019-T038) - **CRITICAL GATE**
3. Complete Phase 3: User Story 1 (T039-T070)
4. Complete Phase 4: User Story 4 (T071-T082)
5. **STOP and VALIDATE**: Test work items display and refresh independently
6. Deploy/demo MVP if ready

**MVP Delivers**: Users can connect to TFS, view assigned work items, open them in browser/VS, and refresh data

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → **MVP Milestone** (view work items)
3. Add User Story 4 → Test independently → **Enhanced MVP** (with refresh)
4. Add User Story 2 → Test independently → **Feature Complete v1** (with pull requests)
5. Add User Story 3 → Test independently → **Feature Complete v2** (with code reviews)
-6. (Removed) System Tray out-of-scope – skip to Performance after core stories
7. Add Performance + Polish → **Production Ready**

Each increment adds value without breaking previous stories.

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together (T001-T038)
2. Once Foundational is done:
   - Developer A: User Story 1 (T039-T070) → PRIORITY (MVP blocker)
   - Developer B: Start on System Tray infrastructure (T117-T119) in parallel
3. After US1 complete:
   - Developer A: User Story 4 (T071-T082)
   - Developer B: User Story 2 (T083-T099)
   - Developer C: User Story 3 (T100-T116)
4. Team reconvenes for Performance (T127-T138) and Polish (T139-T158)

---

## Notes

- **[P] tasks** = different files, no dependencies, can run in parallel
- **[Story] label** maps task to specific user story for traceability
- **Each user story** should be independently completable and testable
- **No tests included** - specification does not explicitly request TDD approach
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- MVP = User Story 1 + User Story 4 (view work items + refresh)
- Performance targets: <2s load, <5s refresh, <100MB memory, 60fps UI
- All paths use Windows-style backslashes as per OS requirements

---

## Task Count Summary

 - **Total Tasks**: 156
 - **Phase 1 (Setup)**: 18 tasks
 - **Phase 2 (Foundational)**: 23 tasks (BLOCKS all user stories)
 - **Phase 3 (User Story 1)**: 35 tasks - **MVP CORE**
 - **Phase 4 (User Story 4)**: 13 tasks - **MVP ENHANCEMENT**
 - **Phase 5 (User Story 2)**: 17 tasks
 - **Phase 6 (User Story 3)**: 17 tasks
 - **Phase 7 (Removed)**: 0 tasks
 - **Phase 8 (Performance)**: 8 tasks
 - **Phase 9 (Polish)**: 15 tasks
 - **Remediation Additions**: 13 tasks

**MVP Scope**: Setup (18) + Foundational (20) + US1 (32) + US4 (12) + Remediation core (T159-T160) = **84 tasks**

**Parallel Opportunities**: 35+ tasks marked [P] can run in parallel when dependencies met

**Independent Test Criteria**:
- **US1**: Connect to TFS with work items, verify display, test browser/VS launch
- **US4**: Modify TFS data, refresh, verify updates; test offline error handling
- **US2**: Create PR assigned to user, verify display in tab, test browser/VS launch
- **US3**: Assign code review to user, verify display in tab, test browser/VS launch

---

## Remediation Additions (New Tasks)

- [X] T159 Implement AutoRefreshTimer (5 min) in MainViewModel (FR-016, SC-008) in src/TfsViewer.App/ViewModels/MainViewModel.cs
 - [X] T160 Add CancelCommand to each tab ViewModel (WorkItems, PullRequests, CodeReviews) to stop in-flight loads (FR-026, SC-014)
- [ ] T161 Implement VsDetectionErrorDialog with fallback message in src/TfsViewer.App/Views/VsDetectionErrorDialog.xaml (FR-023, SC-005)
- [ ] T162 Add ReadOnlyAudit script to verify no mutation endpoints invoked (SC-010) in scripts/ReadOnlyAudit.ps1
- [X] T163 Refactor code references from StoriesTabViewModel to WorkItemsTabViewModel across src/TfsViewer.App/ViewModels/
- [ ] T164 Standardize cache TTL to 5 minutes for all item types (update pull request TTL from 2m) in src/TfsViewer.Core/Services/TfsService.cs
- [ ] T165 Add UsabilitySmokeTest for first-run success (SC-011) in tests/Usability/UsabilitySmokeTest.cs
 
 - [ ] T166 [P] Update IsVisualStudioInstalled method in LauncherService to detect VS 2022 specifically via registry key HKLM\SOFTWARE\Microsoft\VisualStudio\17.0 (FR-029)
 - [ ] T167 [P] Add FR-031 verification: Ensure no mutex or single-instance enforcement in App.xaml.cs (allow multiple instances)
 - [ ] T168 Apply Polly retry policy to TfsService.GetPullRequestsAsync with same configuration as T081a (FR-028)
 - [ ] T169 Apply Polly retry policy to TfsService.GetCodeReviewsAsync with same configuration as T081a (FR-028)
 - [ ] T170 Add LoggingService integration to PullRequestTabViewModel for error logging (FR-030)
 - [ ] T171 Add LoggingService integration to CodeReviewTabViewModel for error logging (FR-030)
