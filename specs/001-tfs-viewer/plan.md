# Implementation Plan: TFS Read-Only Viewer Application

**Branch**: `001-tfs-viewer` | **Date**: 2025-11-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-tfs-viewer/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

A Windows desktop application built with WPF that provides read-only access to TFS work items, pull requests, and code reviews assigned to the current user. The application uses Windows Authentication, displays items in a responsive UI with automatic 5-minute refresh, and allows opening items in browser or Visual Studio. Primary goal: centralize view of assigned TFS items without opening multiple tools.

## Technical Context

**Language/Version**: C# / .NET 6.0 or later  
**Primary Dependencies**: Microsoft.TeamFoundationServer.Client, Microsoft.VisualStudio.Services.Client, System.Windows (WPF)  
**Storage**: Local application settings for TFS server URL (user preferences); no database required  
**Testing**: xUnit for unit tests, WPF UI automation for integration tests  
**Target Platform**: Windows 10/11 desktop (x64)
**Project Type**: Single desktop application (WPF)  
**Performance Goals**: <5s initial load, <10s refresh, UI responsive during all operations  
**Constraints**: Read-only (no TFS modifications), Windows Authentication only, <200MB memory footprint  
**Scale/Scope**: Support up to 500 assigned items per user, single main window with tab navigation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Clarity First ✅ PASS
- Feature spec includes clear functional requirements (FR-001 through FR-026)
- All requirements written in plain language
- Acceptance criteria are testable and measurable (SC-001 through SC-014)
- All ambiguities resolved through clarification session (6 Q&A pairs documented)

### II. Build What's Needed ✅ PASS
- Scope limited to read-only viewing (FR-017)
- No unnecessary features beyond specification
- Simple desktop application using standard WPF framework
- No complex architecture patterns introduced without justification
- Authentication uses existing Windows credentials (no custom auth system)

### III. Track Progress ✅ PASS
- User stories prioritized (P1, P2, P3)
- Each story independently testable
- Success criteria provide clear validation checkpoints
- Implementation will be broken into phases (0: Research, 1: Design, 2: Tasks)

**Overall Assessment**: ✅ ALL GATES PASSED - Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── TfsViewer/                    # Main WPF application project
│   ├── App.xaml                  # Application entry point
│   ├── App.xaml.cs
│   ├── MainWindow.xaml           # Main application window
│   ├── MainWindow.xaml.cs
│   ├── Models/                   # Data models
│   │   ├── WorkItem.cs
│   │   ├── PullRequest.cs
│   │   ├── CodeReview.cs
│   │   └── TfsConnection.cs
│   ├── Services/                 # Business logic and TFS integration
│   │   ├── ITfsService.cs
│   │   ├── TfsService.cs
│   │   ├── IVisualStudioDetector.cs
│   │   └── VisualStudioDetector.cs
│   ├── ViewModels/               # MVVM view models
│   │   ├── MainViewModel.cs
│   │   ├── WorkItemsViewModel.cs
│   │   ├── PullRequestsViewModel.cs
│   │   └── CodeReviewsViewModel.cs
│   ├── Views/                    # User controls for each section
│   │   ├── WorkItemsView.xaml
│   │   ├── PullRequestsView.xaml
│   │   └── CodeReviewsView.xaml
│   ├── Converters/               # XAML value converters
│   ├── Helpers/                  # Utility classes
│   │   ├── RelayCommand.cs
│   │   └── NotifyPropertyChanged.cs
│   └── Resources/                # Styles, templates, images
│       └── Styles.xaml

tests/
├── TfsViewer.Tests/              # Unit tests
│   ├── Services/
│   │   ├── TfsServiceTests.cs
│   │   └── VisualStudioDetectorTests.cs
│   └── ViewModels/
│       ├── MainViewModelTests.cs
│       └── WorkItemsViewModelTests.cs
└── TfsViewer.IntegrationTests/   # Integration tests
    └── TfsConnectionTests.cs

TfsViewer.sln                     # Solution file
```

**Structure Decision**: Single WPF desktop application using MVVM pattern. This is a straightforward Windows desktop app with no backend/frontend split needed. The MVVM pattern is standard for WPF applications and provides good separation of concerns for testability while remaining simple.

## Complexity Tracking

No constitution violations detected. All complexity is justified by requirements:
- WPF framework: Specified in requirements for modern Windows desktop UI
- MVVM pattern: Standard WPF best practice for separation of concerns and testability
- TFS SDK dependencies: Required to communicate with TFS server
- No unnecessary abstractions or patterns introduced

---

## Post-Design Constitution Check

*Re-evaluation after Phase 1 (Design & Contracts)*

### I. Clarity First ✅ PASS
- Data model defined with 4 core entities (WorkItem, PullRequest, CodeReview, TfsConnection)
- All entity properties documented with types, validation rules, and constraints
- API contracts defined with clear service interfaces (ITfsService, ICacheService, IDialogService, IVisualStudioDetector)
- Quickstart guide provides clear developer onboarding path
- No ambiguous design decisions remaining

### II. Build What's Needed ✅ PASS
- Design stays within specification boundaries
- 4 entities, 4 service interfaces - minimal necessary complexity
- No repository pattern, no complex domain logic - appropriate for read-only CRUD
- MVVM with CommunityToolkit.Mvvm uses source generators to reduce boilerplate
- Cache strategy is simple (MemoryCache with 5-min TTL)
- No over-engineering detected

### III. Track Progress ✅ PASS
- Phase 0 complete: Research documented with all technical decisions
- Phase 1 complete: Data model, API contracts, quickstart guide created
- Phase 2 ready: Task breakdown will follow this plan
- All artifacts versioned and documented
- Clear dependency graph (Models → Services → ViewModels → Views)

**Overall Assessment**: ✅ ALL GATES PASSED - Design adheres to constitution principles

**Recommendation**: Proceed to Phase 2 - Task breakdown using `/speckit.tasks` command

---

## Deliverables Summary

### Phase 0: Research ✅ COMPLETE
- **File**: `research.md`
- **Content**: TFS SDK selection, Windows Auth patterns, API query strategies, VS integration, performance optimization, WPF/MVVM framework decisions, async loading patterns, timer implementation, error handling, Visual Studio detection
- **Status**: All technical unknowns resolved

### Phase 1: Design & Contracts ✅ COMPLETE
- **File**: `data-model.md`
    - 4 core entities defined
    - Validation rules documented
    - Cache strategy specified
- **File**: `contracts/api-contracts.md`
    - ITfsService (TFS data retrieval)
    - ICacheService (data caching)
    - IDialogService (error handling)
    - IVisualStudioDetector (VS detection)
- **File**: `quickstart.md`
    - Developer setup guide
    - Project structure overview
    - Common tasks and workflows
- **File**: `.github/agents/copilot-instructions.md`
    - Updated with technology stack
    - C# / .NET 6.0, WPF, TFS SDK documented

### Phase 2: Tasks (Next Step)
- **Command**: Run `/speckit.tasks` to generate task breakdown
- **Expected Output**: `tasks.md` with prioritized, independent tasks
- **Task Categories**: Project setup, Models, Services, ViewModels, Views, Testing, Documentation

---

## Next Actions

1. ✅ Phase 0 Research complete
2. ✅ Phase 1 Design & Contracts complete
3. ✅ Agent context updated
4. ✅ Post-design constitution check passed
5. ⏭️ **Next**: Run `/speckit.tasks` to generate implementation tasks
