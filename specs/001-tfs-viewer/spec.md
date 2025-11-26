# Feature Specification: TFS Read-Only Viewer Application

**Feature Branch**: `001-tfs-viewer`  
**Created**: 2025-11-25  
**Status**: Draft  
**Input**: User description: "направи приложение което да ми помогне да работя с tfs server. трябва да поддържа code review, pull request и work items които са assigned към мен. трябва да e read only т.е. само ги показва и позволява отварянето им в браузър или в visual studio"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Assigned Work Items (Priority: P1)

As a developer, I want to see all work items assigned to me in TFS so that I can track my tasks without opening multiple tools or browsers.

**Why this priority**: This is the foundation of the application - viewing assigned work items is the primary use case that provides immediate value. Without this, the application has no purpose.

**Independent Test**: Can be fully tested by connecting to a TFS server with at least one work item assigned to the user and verifying the work item displays with its basic information (ID, title, status, type).

**Acceptance Scenarios**:

1. **Given** I am logged in to the application with valid TFS credentials, **When** I open the application, **Then** I see a list of all work items currently assigned to me
2. **Given** I have work items assigned to me, **When** I view the work items list, **Then** each work item displays its ID, title, type, state, and assigned date
3. **Given** I am viewing a work item in the list, **When** I select "Open in Browser", **Then** the work item opens in my default browser at the TFS web URL
4. **Given** I am viewing a work item in the list, **When** I select "Open in Visual Studio", **Then** the work item opens in Visual Studio

---

### User Story 2 - View Pull Requests (Priority: P2)

As a developer, I want to see all pull requests assigned to me for review so that I can prioritize my code review work without navigating through TFS web interface.

**Why this priority**: Code reviews are a critical part of development workflow. This provides value by centralizing pull request visibility, but work items (P1) are more fundamental to task tracking.

**Independent Test**: Can be fully tested by creating a pull request assigned to the user for review and verifying it appears in the application with relevant details.

**Acceptance Scenarios**:

1. **Given** I am logged in to the application, **When** I navigate to the Pull Requests section, **Then** I see all pull requests where I am a reviewer
2. **Given** I have pending pull requests to review, **When** I view the pull requests list, **Then** each pull request displays its ID, title, author, creation date, and status
3. **Given** I am viewing a pull request in the list, **When** I select "Open in Browser", **Then** the pull request opens in my default browser at the TFS web URL
4. **Given** I am viewing a pull request in the list, **When** I select "Open in Visual Studio", **Then** the pull request opens in Visual Studio for review

---

### User Story 3 - View Code Reviews (Priority: P3)

As a developer, I want to see code reviews assigned to me so that I can track my pending reviews in one place.

**Why this priority**: Code reviews complement pull requests. While important, this is lower priority because modern TFS workflows often use pull requests as the primary review mechanism.

**Independent Test**: Can be fully tested by assigning a code review to the user and verifying it appears in the application with review details.

**Acceptance Scenarios**:

1. **Given** I am logged in to the application, **When** I navigate to the Code Reviews section, **Then** I see all code reviews assigned to me
2. **Given** I have pending code reviews, **When** I view the code reviews list, **Then** each review displays its ID, title, requester, creation date, and status
3. **Given** I am viewing a code review in the list, **When** I select "Open in Browser", **Then** the code review opens in my default browser at the TFS web URL
4. **Given** I am viewing a code review in the list, **When** I select "Open in Visual Studio", **Then** the code review opens in Visual Studio

---

### User Story 4 - Refresh Data (Priority: P2)

As a developer, I want to refresh the displayed data from TFS so that I always see the most current information about my assigned items.

**Why this priority**: Essential for data accuracy, but secondary to the core viewing functionality. Users need current data to make decisions.

**Independent Test**: Can be fully tested by modifying data on TFS server and verifying the refresh action retrieves and displays the updated information.

**Acceptance Scenarios**:

1. **Given** I am viewing any section of the application, **When** I trigger a refresh action, **Then** the application retrieves the latest data from TFS server
2. **Given** data has changed on TFS server since last refresh, **When** I refresh the data, **Then** I see the updated information reflected in the application
3. **Given** the TFS server is temporarily unavailable, **When** I attempt to refresh, **Then** I see a clear error message indicating connection failure

---

### Edge Cases

- What happens when the TFS server is unreachable or offline?
- How does the system handle authentication failures or expired credentials?
- What happens when a user has no assigned work items, pull requests, or code reviews?
- How does the system handle work items or pull requests that were deleted on the server?
- How does the system handle very large numbers of assigned items (e.g., 1000+ work items)?
- What happens when TFS server permissions change and user loses access to previously visible items?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST connect to TFS server using user-provided credentials
- **FR-002**: System MUST accept TFS server URL in full collection URL format (e.g., http://tfs.company.com:8080/tfs/DefaultCollection)
- **FR-003**: System MUST retrieve and display all work items assigned to the authenticated user
- **FR-004**: System MUST retrieve and display all pull requests where the authenticated user is a reviewer
- **FR-005**: System MUST retrieve and display all code reviews assigned to the authenticated user
- **FR-006**: System MUST display work item details including: ID, title, type, state, and assigned date
- **FR-007**: System MUST display pull request details including: ID, title, author, creation date, and status
- **FR-008**: System MUST display code review details including: ID, title, requester, creation date, and status
- **FR-009**: System MUST provide an action to open any work item in the default web browser at its TFS URL
- **FR-010**: System MUST provide an action to open any work item in Visual Studio
- **FR-011**: System MUST provide an action to open any pull request in the default web browser at its TFS URL
- **FR-012**: System MUST provide an action to open any pull request in Visual Studio
- **FR-013**: System MUST provide an action to open any code review in the default web browser at its TFS URL
- **FR-014**: System MUST provide an action to open any code review in Visual Studio
- **FR-015**: System MUST provide a manual refresh mechanism to retrieve the latest data from TFS server on demand
- **FR-016**: System MUST automatically refresh data from TFS server every 5 minutes when the application is running
- **FR-017**: System MUST be read-only and MUST NOT allow creating, editing, or deleting any TFS items
- **FR-018**: System MUST display clear error messages when TFS server is unreachable
- **FR-019**: System MUST display clear error messages when authentication fails
- **FR-020**: System MUST handle scenarios where user has no assigned items gracefully with appropriate messaging
- **FR-021**: System MUST authenticate to TFS server using Windows Authentication with the current user's credentials
- **FR-022**: System MUST validate TFS server connection before attempting to retrieve data
- **FR-023**: When Visual Studio is not detected on the system, attempting to open an item in Visual Studio MUST display an error message "Visual Studio not detected" and offer to open the item in browser instead
- **FR-024**: System MUST display a loading indicator or progress bar during data retrieval operations
- **FR-025**: System MUST keep the UI responsive during data loading operations
- **FR-026**: System MUST provide a cancel option for ongoing data retrieval operations

### Key Entities *(include if feature involves data)*

- **Work Item**: Represents a TFS work item with attributes: ID (unique identifier), title (brief description), type (bug, task, user story, etc.), state (new, active, resolved, closed, etc.), assigned date (when assigned to user), TFS URL (web link), Visual Studio link
- **Pull Request**: Represents a code review request with attributes: ID (unique identifier), title (description of changes), author (who created the pull request), creation date, status (active, completed, abandoned), reviewer list (includes current user), TFS URL, Visual Studio link
- **Code Review**: Represents a formal code review with attributes: ID (unique identifier), title (review description), requester (who requested the review), creation date, status (pending, completed), assigned reviewer (current user), TFS URL, Visual Studio link
- **TFS Connection**: Represents the connection to TFS server with attributes: server URL (full collection URL format, e.g., http://tfs.company.com:8080/tfs/DefaultCollection), user credentials, connection status, last successful refresh timestamp

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view all their assigned work items within 5 seconds of opening the application
- **SC-002**: Users can view all their assigned pull requests within 5 seconds of navigating to the pull requests section
- **SC-003**: Users can view all their assigned code reviews within 5 seconds of navigating to the code reviews section
- **SC-004**: 100% of work items, pull requests, and code reviews successfully open in browser when requested
- **SC-005**: 100% of work items, pull requests, and code reviews successfully open in Visual Studio when requested and Visual Studio is installed; when Visual Studio is not installed, user receives clear error message and browser fallback option
- **SC-006**: Application successfully handles TFS servers with up to 500 assigned items per user without performance degradation
- **SC-007**: Users can manually refresh data and see updates within 10 seconds
- **SC-008**: Application automatically refreshes data every 5 minutes without user intervention
- **SC-009**: Application displays clear, actionable error messages for 100% of connection and authentication failures
- **SC-010**: Application never allows modification, creation, or deletion of TFS items (100% read-only verification)
- **SC-011**: Users can complete their primary task (viewing assigned items) on first use without external help
- **SC-012**: Application displays loading indicators for 100% of data retrieval operations taking longer than 1 second
- **SC-013**: UI remains responsive during all loading operations (no UI freezing)
- **SC-014**: Users can successfully cancel any data loading operation in progress

## Assumptions

- Users have valid TFS server credentials and permissions to access their assigned items
- Users have network access to the TFS server
- TFS server uses standard TFS/Azure DevOps Server APIs
- Visual Studio is installed on the user's machine for "Open in Visual Studio" functionality to work
- Users work primarily in a Windows environment where Visual Studio integration is standard
- TFS server version is 2015 or later (supports modern REST APIs)
- Application uses Windows Authentication with current user's credentials (no separate credential storage required)
- Application will be a desktop application (not web-based) based on "Open in Visual Studio" requirement
- Application will be built using WPF (Windows Presentation Foundation) for modern Windows desktop UI
- Users typically have fewer than 500 assigned items at any given time
- Network latency to TFS server is within typical corporate network ranges (< 100ms)

## Clarifications

### Session 2025-11-25

- Q: FR-019 - How should TFS credentials be stored (OS credential manager, encrypted local storage, or session-only)? → A: Use Windows Authentication with current user credentials
- Q: What should happen when Visual Studio is not installed and user clicks "Open in Visual Studio"? → A: Show error message "Visual Studio not detected" and offer to open in browser instead
- Q: What TFS server URL format should be expected for configuration? → A: Full collection URL (e.g., http://tfs.company.com:8080/tfs/DefaultCollection)
- Q: What refresh strategy should the application use? → A: Auto-refresh every 5 minutes plus manual refresh button
- Q: What UI technology/framework should be used for the desktop application? → A: WPF (modern Windows desktop, rich UI, XAML-based)
- Q: What should happen during data loading operations? → A: Show loading indicator/progress bar, keep UI responsive, allow cancel
