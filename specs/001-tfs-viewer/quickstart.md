# Quickstart Guide: TFS Read-Only Viewer

**Feature**: 001-tfs-viewer  
**Date**: 2025-11-25  
**For**: Developers working on the TFS Read-Only Viewer application

## Overview

This quickstart guide helps developers set up their development environment and understand the codebase structure for the TFS Read-Only Viewer Windows desktop application.

---

## Prerequisites

### Required Software

1. **Visual Studio 2022** (Community, Professional, or Enterprise)
   - Workload: ".NET Desktop Development"
   - Components: .NET 6 SDK or .NET 8 SDK

2. **Windows 10 or Windows 11**
   - Minimum version: Windows 10 1809 (build 17763)

3. **Git** for version control

### Optional Software

- **Visual Studio Code** with C# extension (alternative IDE)
- **TFS Server** or **Azure DevOps Server** for testing (or access to company TFS)

---

## Project Setup

### 1. Clone Repository

```powershell
git clone <repository-url>
cd plam_tfs_wi
git checkout 001-tfs-viewer
```

### 2. Install Dependencies

Dependencies are managed via NuGet. They will be restored automatically when you build the project.

**Key NuGet Packages**:

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.TeamFoundationServer.Client` | Latest | TFS REST API client |
| `MaterialDesignThemes` | 4.9+ | Modern Material Design UI |
| `MaterialDesignColors` | 2.1+ | Color themes |
| `CommunityToolkit.Mvvm` | 8.2+ | MVVM helpers (source generators) |
| `Hardcodet.NotifyIcon.Wpf` | 1.1+ | System tray integration |
| `CredentialManagement` | 1.0.2 | Windows Credential Manager wrapper |
| `System.Runtime.Caching` | Built-in | In-memory caching |
| `xUnit` | Latest | Unit testing framework |
| `Moq` | Latest | Mocking library |
| `FlaUI.UIA3` | 4.0+ | UI automation testing |

### 3. Build Solution

**Option A: Visual Studio**
1. Open `TfsViewer.sln` in Visual Studio 2022
2. Set `TfsViewer.App` as startup project
3. Press **F5** to build and run

**Option B: Command Line**
```powershell
dotnet restore
dotnet build
dotnet run --project src/TfsViewer.App/TfsViewer.App.csproj
```

### 4. Configure TFS Connection (First Run)

On first run, the app will prompt for TFS connection settings:

1. **Server URL**: `https://tfs.company.com/DefaultCollection` (or your TFS server URL)
2. **Authentication Type**: Choose "Personal Access Token" or "Windows Authentication"
3. **Personal Access Token** (if selected): Enter your TFS PAT with these scopes:
   - Work Items: Read
   - Code: Read
   - Pull Request Threads: Read

**Generate PAT**:
- TFS/Azure DevOps: User Settings → Security → Personal Access Tokens → New Token
- Scopes: Work Items (Read), Code (Read)

---

## Project Structure

```
plam_tfs_wi/
├── src/
│   ├── TfsViewer.App/              # Main WPF application
│   │   ├── App.xaml                # Application entry point
│   │   ├── Views/                  # XAML views
│   │   │   ├── MainWindow.xaml     # Main tabbed UI
│   │   │   ├── SettingsWindow.xaml # Connection settings
│   │   │   └── DetailViews/        # Item detail dialogs
│   │   ├── ViewModels/             # MVVM ViewModels
│   │   │   ├── MainViewModel.cs
│   │   │   ├── CodeReviewTabViewModel.cs
│   │   │   ├── PullRequestTabViewModel.cs
│   │   │   └── StoriesTabViewModel.cs
│   │   ├── Services/               # UI-specific services
│   │   │   ├── LauncherService.cs  # Open in VS/Browser
│   │   │   └── TrayIconManager.cs  # System tray
│   │   ├── Infrastructure/         # Cross-cutting
│   │   │   ├── Configuration.cs
│   │   │   └── CredentialStore.cs
│   │   └── Resources/              # Styles, icons, themes
│   │
│   └── TfsViewer.Core/             # Core business logic (no UI deps)
│       ├── Api/                    # TFS API clients
│       │   └── TfsApiClient.cs
│       ├── Models/                 # Domain models
│       │   ├── WorkItem.cs
│       │   ├── PullRequest.cs
│       │   ├── CodeReview.cs
│       │   └── TfsConnection.cs
│       ├── Services/               # Core services
│       │   ├── TfsService.cs       # Main TFS data service
│       │   └── CacheService.cs     # Data caching
│       └── Contracts/              # Interfaces
│           ├── ITfsService.cs
│           └── ICacheService.cs
│
├── tests/
│   ├── TfsViewer.App.Tests/        # UI & integration tests
│   │   ├── ViewModels/
│   │   └── Integration/
│   └── TfsViewer.Core.Tests/       # Unit tests
│       ├── Services/
│       └── Api/
│
└── specs/
    └── 001-tfs-viewer/             # Feature documentation
        ├── spec.md                 # Feature specification
        ├── plan.md                 # Implementation plan
        ├── research.md             # Technical research
        ├── data-model.md           # Data model
        ├── contracts/              # API contracts
        └── quickstart.md           # This file
```

---

## Key Architectural Patterns

### 1. MVVM (Model-View-ViewModel)

**Example: Displaying Work Items**

**Model** (`WorkItem.cs`):
```csharp
public class WorkItem
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Type { get; init; }
    public string State { get; init; }
    public DateTime AssignedDate { get; init; }
    // ... other properties
}
```

**ViewModel** (`StoriesTabViewModel.cs`):
```csharp
public partial class StoriesTabViewModel : ObservableObject
{
    private readonly ITfsService _tfsService;
    
    [ObservableProperty]
    private ObservableCollection<WorkItemViewModel> _workItems = new();
    
    public int WorkItemCount => WorkItems.Count;
    
    [RelayCommand]
    private async Task LoadWorkItemsAsync()
    {
        var items = await _tfsService.GetAssignedWorkItemsAsync();
        WorkItems.Clear();
        foreach (var item in items)
            WorkItems.Add(new WorkItemViewModel(item));
    }
}
```

**View** (`MainWindow.xaml`):
```xaml
<TabControl>
    <TabItem Header="{Binding WorkItemCount, StringFormat='Stories ({0})'}">
        <ListView ItemsSource="{Binding WorkItems}">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <StackPanel>
                        <TextBlock Text="{Binding Title}" FontWeight="Bold"/>
                        <TextBlock Text="{Binding State}"/>
                    </StackPanel>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
    </TabItem>
</TabControl>
```

### 2. Dependency Injection

Using `Microsoft.Extensions.DependencyInjection`:

**App.xaml.cs**:
```csharp
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<ITfsService, TfsService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<ICredentialStore, CredentialStore>();
        
        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        
        // Views
        services.AddSingleton<MainWindow>();
    }
}
```

### 3. Async/Await Pattern

All TFS API calls are asynchronous:

```csharp
public async Task<IReadOnlyList<WorkItem>> GetAssignedWorkItemsAsync(
    CancellationToken cancellationToken = default)
{
    // Check cache first
    var cached = _cacheService.Get<WorkItem>("workitems");
    if (cached != null)
        return cached;
    
    // Fetch from TFS
    var items = await _tfsApiClient.GetWorkItemsAsync(cancellationToken);
    
    // Update cache
    _cacheService.Set("workitems", items, TimeSpan.FromMinutes(5));
    
    return items;
}
```

### 4. Caching Strategy

**Two-tier cache**:
1. **In-memory**: `MemoryCache` for fast access
2. **Disk**: JSON files for offline/startup

**Cache Service Usage**:
```csharp
// Set cache with TTL
_cacheService.Set("pullrequests", items, TimeSpan.FromMinutes(2));

// Get from cache
var items = _cacheService.Get<PullRequest>("pullrequests");
if (items == null)
{
    // Cache miss, fetch from TFS
    items = await _tfsService.GetPullRequestsAsync();
}
```

---

## Common Development Tasks

### Task 1: Add a New Tab

1. **Create ViewModel**: `src/TfsViewer.App/ViewModels/NewTabViewModel.cs`
2. **Create View**: `src/TfsViewer.App/Views/NewTabView.xaml`
3. **Update MainViewModel**: Add property for new tab ViewModel
4. **Update MainWindow.xaml**: Add new `<TabItem>` to TabControl
5. **Register in DI**: Add to `App.xaml.cs` ConfigureServices

### Task 2: Add a New TFS API Endpoint

1. **Define interface**: Add method to `ITfsService`
2. **Implement in TfsService**: Add async method with TFS API call
3. **Add caching**: Use `ICacheService` to cache results
4. **Create ViewModel method**: Call service from ViewModel
5. **Bind in View**: Display data in XAML

### Task 3: Add a New Action Button

1. **Add RelayCommand to ViewModel**:
   ```csharp
   [RelayCommand]
   private void DoSomething(WorkItem item)
   {
       // Action logic
   }
   ```
2. **Bind in XAML**:
   ```xaml
   <Button Content="Do Something" 
           Command="{Binding DoSomethingCommand}" 
           CommandParameter="{Binding}"/>
   ```

### Task 4: Debug TFS API Calls

1. Set breakpoint in `TfsService.cs`
2. Run with F5 (Debug mode)
3. Inspect `_tfsApiClient` responses
4. Check `Output` window for logs

**Enable detailed logging**:
```csharp
// In TfsApiClient constructor
var handler = new HttpClientHandler { UseDefaultCredentials = true };
var client = new HttpClient(handler) { BaseAddress = new Uri(serverUrl) };
client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
```

---

## Running Tests

### Unit Tests

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/TfsViewer.Core.Tests/TfsViewer.Core.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### UI Tests (FlaUI)

**Prerequisites**: Application must be built in Release mode

```powershell
dotnet build -c Release
dotnet test tests/TfsViewer.App.Tests/TfsViewer.App.Tests.csproj --filter Category=UI
```

**Example UI Test**:
```csharp
[Fact]
public void MainWindow_Shows_Three_Tabs()
{
    using var app = Application.Launch("TfsViewer.App.exe");
    using var automation = new UIA3Automation();
    var window = app.GetMainWindow(automation);
    
    var tabs = window.FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem));
    Assert.Equal(3, tabs.Length);
}
```

---

## Performance Profiling

### Memory Profiling

**Using Visual Studio**:
1. Debug → Performance Profiler
2. Select ".NET Object Allocation Tracking"
3. Start profiling
4. Perform actions (load data, switch tabs)
5. Stop profiling and analyze report

**Target**: <100MB total memory usage

### CPU Profiling

**Using Visual Studio**:
1. Debug → Performance Profiler
2. Select "CPU Usage"
3. Start profiling
4. Measure refresh operation
5. Check hot paths (should be in TFS API calls, not UI)

**Target**: UI thread <10% CPU during idle, <50% during refresh

---

## Debugging Tips

### Visual Studio Not Launching Items

**Problem**: "Open in Visual Studio" button does nothing

**Debug Steps**:
1. Check if VS is installed: `LauncherService.IsVisualStudioInstalled()`
2. Verify protocol handler registration:
   ```powershell
   Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\VisualStudio\*" | Select-Object -Property PSChildName
   ```
3. Test protocol manually:
   ```powershell
   Start-Process "vstfs:///WorkItemTracking/WorkItem/12345?url=https://tfs.company.com"
   ```

### TFS Connection Fails

**Problem**: Cannot connect to TFS server

**Debug Steps**:
1. Verify URL in browser: Navigate to `https://tfs.company.com/DefaultCollection`
2. Check network: `Test-NetConnection -ComputerName tfs.company.com -Port 443`
3. Verify credentials: Try API call with `curl`:
   ```powershell
   $token = "your-pat-token"
   $base64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$token"))
   curl -H "Authorization: Basic $base64" https://tfs.company.com/DefaultCollection/_apis/connectionData
   ```
4. Check TFS service logs (if you have admin access)

### UI Not Updating After Refresh

**Problem**: Data refreshes but UI doesn't update

**Debug Steps**:
1. Verify ViewModel uses `ObservableCollection<T>`
2. Check that properties raise `PropertyChanged` (use `[ObservableProperty]` attribute)
3. Ensure updates happen on UI thread:
   ```csharp
   Application.Current.Dispatcher.Invoke(() => {
       WorkItems.Clear();
       WorkItems.Add(newItem);
   });
   ```

---

## Code Style & Conventions

### Naming Conventions

- **Classes**: PascalCase (`WorkItem`, `TfsService`)
- **Interfaces**: IPascalCase (`ITfsService`, `ICacheService`)
- **Methods**: PascalCase (`GetWorkItemsAsync`)
- **Properties**: PascalCase (`WorkItemCount`)
- **Fields**: _camelCase (`_tfsService`, `_cacheService`)
- **Parameters**: camelCase (`cancellationToken`, `serverUrl`)

### Async Methods

- Always suffix with `Async`: `LoadDataAsync()`, `ConnectAsync()`
- Always return `Task` or `Task<T>`
- Always accept `CancellationToken cancellationToken = default`

### Error Handling

```csharp
try
{
    var items = await _tfsService.GetWorkItemsAsync(cancellationToken);
}
catch (TfsServiceException ex)
{
    // User-friendly error message
    MessageBox.Show(ex.UserMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    Logger.LogError(ex, "Failed to fetch work items");
}
catch (Exception ex)
{
    // Unexpected error
    MessageBox.Show("An unexpected error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    Logger.LogError(ex, "Unexpected error in GetWorkItemsAsync");
}
```

---

## Frequently Asked Questions

### Q: How do I change the UI theme?

**A**: Edit `appsettings.json`:
```json
{
  "TfsViewer": {
    "UI": {
      "Theme": "Dark",  // "Light" or "Dark"
      "AccentColor": "#0078D4"
    }
  }
}
```

### Q: Can I test without a TFS server?

**A**: Yes, use mock data:
1. Create `MockTfsService.cs` implementing `ITfsService`
2. Return hardcoded test data
3. Register in DI: `services.AddSingleton<ITfsService, MockTfsService>();`

### Q: How do I add a new field to WorkItem?

**A**:
1. Add property to `WorkItem.cs` model
2. Update `TfsApiClient.cs` to parse new field from TFS response
3. Update `WorkItemViewModel.cs` to expose new field
4. Update XAML DataTemplate to display new field

### Q: Performance is slow with 500+ items. How to optimize?

**A**:
1. Ensure `VirtualizingStackPanel` is used in ListView
2. Simplify DataTemplate (fewer controls)
3. Use `RecyclingMode="Recycling"` on ListView
4. Implement pagination (fetch only first 100, load more on scroll)

---

## Resources

### Official Documentation

- **TFS REST API**: https://docs.microsoft.com/en-us/rest/api/azure/devops/
- **WPF Documentation**: https://docs.microsoft.com/en-us/dotnet/desktop/wpf/
- **Material Design in XAML**: http://materialdesigninxaml.net/
- **CommunityToolkit.Mvvm**: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/

### Useful Tools

- **REST Client**: Postman or Insomnia (test TFS API calls)
- **XAML Spy**: Inspect WPF visual tree at runtime
- **dotMemory**: Advanced memory profiling (JetBrains)
- **BenchmarkDotNet**: Performance benchmarking for .NET

---

## Next Steps

1. ✅ Read `spec.md` to understand feature requirements
2. ✅ Read `research.md` for technical decisions
3. ✅ Review `data-model.md` for entity definitions
4. ✅ Review `contracts/api-contracts.md` for API interfaces
5. ⏭️ Wait for `tasks.md` to be generated (`/speckit.tasks` command)
6. ⏭️ Start implementing tasks in priority order
7. ⏭️ Write tests for each feature
8. ⏭️ Profile and optimize performance

---

## Getting Help

- **Code Questions**: Check this quickstart or `contracts/api-contracts.md`
- **Feature Clarifications**: Refer to `spec.md`
- **Technical Decisions**: See `research.md`
- **API Usage**: See `contracts/api-contracts.md`

Happy coding! 🚀
