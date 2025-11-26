# Implementation Plan: TFS Read-Only Viewer Application

**Branch**: `001-tfs-viewer` | **Date**: 2025-11-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-tfs-viewer/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a Windows desktop application that displays TFS work items, pull requests, and code reviews assigned to the current user. The application is strictly read-only, using WPF for the UI, Windows Authentication for TFS access, and Visual Studio 2022 integration for opening items. Key features include 5-minute auto-refresh, manual refresh with retry logic (3x exponential backoff), basic file-based error/warning logging, and support for up to 500 items without performance degradation.

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: WPF, MaterialDesignThemes 5.1.0, CommunityToolkit.Mvvm 8.3.2, Microsoft.TeamFoundationServer.Client 19.250.0-preview, Microsoft.VisualStudio.Services.Client 19.250.0-preview  
**Storage**: System.Runtime.Caching (in-memory cache for TFS data), local file for logging  
**Testing**: MSTest or xUnit (unit tests), manual acceptance testing per user story scenarios  
**Target Platform**: Windows 10+ desktop (net10.0-windows)  
**Project Type**: Desktop application (WPF single executable with supporting class library)  
**Performance Goals**: Load 500 items within 5 seconds, UI responsive during all operations, auto-refresh every 5 minutes  
**Constraints**: Read-only (no TFS modifications), 3x retry with exponential backoff on network failures, <5s initial load, <10s manual refresh  
**Scale/Scope**: Up to 500 work items/PRs/code reviews per user, single TFS collection connection, VS 2022 integration only

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Initial Check (Pre-Phase 0)**:

| Principle | Status | Verification |
|-----------|--------|--------------|
| **I. Clarity First** | ✅ PASS | Spec contains 4 user stories with Given/When/Then acceptance scenarios, 31 functional requirements (FR-001 to FR-031), 14 measurable success criteria, 11 assumptions, and 2 clarification sessions (11 Q&A pairs total). All requirements testable and written in plain language. |
| **II. Build What's Needed** | ✅ PASS | Scope limited to read-only viewing of 3 TFS item types (work items, PRs, code reviews) with browser/VS opening actions. No creation, editing, or deletion features. Simple auto-refresh (5 min) and manual refresh. Basic file logging only. VS 2022 only (not multi-version). No unnecessary abstractions specified. |
| **III. Track Progress** | ✅ PASS | Existing tasks.md has 146 tasks with dependencies marked, 88 tasks (60%) already completed with checkpoints validated. Remaining work organized in phases (Phase 5 PRs, Phase 6 Code Reviews, Phase 8 Performance, Phase 9 Polish). |

**Post-Phase 1 Re-Check**:

| Principle | Status | Verification |
|-----------|--------|--------------|
| **I. Clarity First** | ✅ PASS | Research.md documents all technology choices with alternatives considered and rationales. Data-model.md defines 6 entities with properties, validation rules, invariants, and lifecycle. API contracts specify TFS endpoints and Visual Studio integration patterns. No ambiguities remain. |
| **II. Build What's Needed** | ✅ PASS | Technology stack remains minimal: WPF (UI), TFS Client SDK (API access), MemoryCache (caching), Serilog (logging), Polly (retry). No over-engineering detected. Two-project structure (App + Core) justified for testability without unnecessary layers. |
| **III. Track Progress** | ✅ PASS | Phase 0 (research.md) and Phase 1 (data-model.md, contracts/, quickstart.md) artifacts complete. Agent context updated. Ready for Phase 2 (tasks.md generation via /speckit.tasks). |

**Overall Gate Status**: ✅ PASS - Proceed to Phase 2 (Task Breakdown)

**Justification for Complexity**: None required - project adheres to all constitution principles without violations.

## Project Structure

### Documentation (this feature)

```text
specs/001-tfs-viewer/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── api-contracts.md # TFS API endpoints and Visual Studio integration contracts
├── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
├── spec.md              # Source specification (already exists)
└── checklists/
    └── requirements.md  # Pre-planning validation checklist
```

### Source Code (repository root)

```text
TfsViewer.sln            # Visual Studio solution file

src/TfsViewer.App/       # WPF application project (net10.0-windows)
├── App.xaml             # Application entry point and resources
├── App.xaml.cs
├── Views/               # XAML views (MainWindow, WorkItemsView, PullRequestsView, CodeReviewsView)
├── ViewModels/          # View models with MVVM pattern (CommunityToolkit.Mvvm)
├── Converters/          # XAML value converters
└── Resources/           # Styles, templates, images

src/TfsViewer.Core/      # Class library project (net10.0)
├── Models/              # Domain entities (WorkItem, PullRequest, CodeReview, TfsConnection)
├── Services/            # Business logic (TfsDataService, CacheService, VisualStudioService, LoggingService)
├── Infrastructure/      # Cross-cutting (TfsClientFactory, RetryPolicy)
└── Configuration/       # App settings and configuration

tests/                   # Test projects (to be created in Phase 8)
├── TfsViewer.Tests/     # Unit tests
└── TfsViewer.IntegrationTests/  # Integration tests with TFS
```

**Structure Decision**: Desktop application structure chosen based on WPF requirement from spec (FR-017, Assumptions section). Two-project approach separates UI concerns (TfsViewer.App) from business logic (TfsViewer.Core), enabling testability and potential future reuse of core logic. This aligns with standard WPF MVVM architecture patterns.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations identified** - All constitution principles satisfied without requiring complexity justification.
