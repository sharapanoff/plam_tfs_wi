# WPF MVVM Best Practices Research Findings

**Date:** November 25, 2025  
**Purpose:** Desktop application for TFS work item viewing with auto-refresh capabilities  
**Target Framework:** .NET 6+ WPF

---

## 1. MVVM Framework Selection

### Decision
**Use CommunityToolkit.Mvvm (Microsoft MVVM Toolkit)**

### Rationale
- **Official Microsoft support**: Part of .NET Community Toolkit, maintained by Microsoft
- **Modern and performant**: Built specifically for .NET Standard 2.0+, .NET 6+ with optimizations
- **Source generators**: Uses C# source generators to reduce boilerplate code
- **Platform agnostic**: Works across WPF, UWP, WinUI 3, Xamarin, Uno, etc.
- **Minimal dependencies**: Lightweight, no strict requirements on application structure
- **Active development**: Used by first-party Microsoft applications (Microsoft Store)
- **Strong community**: Part of .NET Foundation with excellent documentation

### Alternatives Considered

**Prism**
- Pros: Full-featured framework with navigation, modularity, dependency injection
- Cons: Heavier framework, more opinionated, steeper learning curve, may be overkill for simpler apps
- Use case: Better for complex, modular enterprise applications

**MVVM Light**
- Pros: Lightweight, well-established
- Cons: No longer actively maintained (archived in 2021), no .NET 6+ optimizations
- Verdict: Avoid for new projects

**Manual Implementation**
- Pros: Full control, no external dependencies, learning experience
- Cons: Time-consuming, error-prone, reinventing the wheel, no source generators
- Verdict: Not recommended for production code

### Implementation Notes

**Installation:**
```bash
dotnet add package CommunityToolkit.Mvvm
```

**Key Features to Use:**

1. **ObservableObject** - Base class for ViewModels
   ```csharp
   public partial class MainViewModel : ObservableObject
   {
       [ObservableProperty]
       private string _userName;
       
       // Source generator creates UserName property with INotifyPropertyChanged
   }
   ```

2. **RelayCommand and AsyncRelayCommand** - For command binding
   ```csharp
   [RelayCommand]
   private async Task LoadDataAsync()
   {
       // Command implementation
   }
   ```

3. **ObservableValidator** - For data validation if needed
4. **Messenger** - For loosely coupled communication between components (if needed)

**Best Practices:**
- Use `partial` classes to enable source generators
- Leverage `[ObservableProperty]` attribute to reduce boilerplate
- Use `[RelayCommand]` for commands
- Keep ViewModels testable (no direct UI dependencies)
- Use constructor injection for services/dependencies

---

## 2. Async Data Loading

### Decision
**Use async/await with Task-based patterns, CancellationToken support, and proper UI thread marshalling**

### Rationale
- **UI Responsiveness**: Prevents UI freezing during long-running operations
- **Modern C# patterns**: Aligns with .NET 6+ best practices
- **Cancellation support**: Allows users to cancel operations (important for network calls)
- **Error handling**: Structured exception handling with try/catch in async methods
- **WPF threading model**: Automatically marshals back to UI thread after await

### Alternatives Considered

**BackgroundWorker**
- Verdict: Legacy approach, avoid in new .NET 6+ applications
- Use async/await instead

**Manual Thread Management**
- Verdict: Too complex, error-prone, unnecessary with async/await

**Task.Run without async/await**
- Verdict: Requires manual Dispatcher.Invoke, less elegant than async/await

### Implementation Notes

**Pattern 1: Async ViewModel Property Initialization**

Use `NotifyTaskCompletion<T>` pattern for data-binding async operations:

```csharp
public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        // Start async load immediately
        WorkItems = new NotifyTaskCompletion<ObservableCollection<WorkItem>>(
            LoadWorkItemsAsync());
    }

    public NotifyTaskCompletion<ObservableCollection<WorkItem>> WorkItems { get; }

    private async Task<ObservableCollection<WorkItem>> LoadWorkItemsAsync()
    {
        await Task.Delay(100); // Small delay for UI to render
        
        try
        {
            var items = await _tfsService.GetWorkItemsAsync(_cancellationTokenSource.Token);
            return new ObservableCollection<WorkItem>(items);
        }
        catch (OperationCanceledException)
        {
            return new ObservableCollection<WorkItem>();
        }
    }
}
```

**Pattern 2: NotifyTaskCompletion Helper Class**

```csharp
public sealed class NotifyTaskCompletion<TResult> : INotifyPropertyChanged
{
    public NotifyTaskCompletion(Task<TResult> task)
    {
        Task = task;
        if (!task.IsCompleted)
        {
            var _ = WatchTaskAsync(task);
        }
    }

    private async Task WatchTaskAsync(Task task)
    {
        try
        {
            await task;
        }
        catch { }
        
        var propertyChanged = PropertyChanged;
        if (propertyChanged == null) return;
        
        propertyChanged(this, new PropertyChangedEventArgs("Status"));
        propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
        propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
        
        if (task.IsCanceled)
        {
            propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
        }
        else if (task.IsFaulted)
        {
            propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
            propertyChanged(this, new PropertyChangedEventArgs("Exception"));
            propertyChanged(this, new PropertyChangedEventArgs("InnerException"));
            propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
        }
        else
        {
            propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
            propertyChanged(this, new PropertyChangedEventArgs("Result"));
        }
    }

    public Task<TResult> Task { get; private set; }
    
    public TResult Result => 
        Task.Status == TaskStatus.RanToCompletion ? Task.Result : default(TResult);
    
    public TaskStatus Status => Task.Status;
    public bool IsCompleted => Task.IsCompleted;
    public bool IsNotCompleted => !Task.IsCompleted;
    public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
    public bool IsCanceled => Task.IsCanceled;
    public bool IsFaulted => Task.IsFaulted;
    public AggregateException Exception => Task.Exception;
    public Exception InnerException => Exception?.InnerException;
    public string ErrorMessage => InnerException?.Message;

    public event PropertyChangedEventHandler PropertyChanged;
}
```

**Pattern 3: Async Commands with CancellationToken**

```csharp
public partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource _loadCts;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Cancel previous operation
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        IsLoading = true;
        StatusMessage = "Loading work items...";

        try
        {
            var items = await _tfsService.GetWorkItemsAsync(_loadCts.Token);
            WorkItems = new ObservableCollection<WorkItem>(items);
            StatusMessage = $"Loaded {items.Count} items";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Load cancelled";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            // Log exception
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

**XAML Data Binding for Async Properties:**

```xml
<Window x:Class="MainWindow">
    <Grid>
        <!-- Busy indicator -->
        <ProgressBar IsIndeterminate="True"
                     Visibility="{Binding WorkItems.IsNotCompleted, 
                                 Converter={StaticResource BoolToVisibilityConverter}}" />
        
        <!-- Results -->
        <ListView ItemsSource="{Binding WorkItems.Result}"
                  Visibility="{Binding WorkItems.IsSuccessfullyCompleted,
                              Converter={StaticResource BoolToVisibilityConverter}}" />
        
        <!-- Error message -->
        <TextBlock Text="{Binding WorkItems.ErrorMessage}"
                   Foreground="Red"
                   Visibility="{Binding WorkItems.IsFaulted,
                               Converter={StaticResource BoolToVisibilityConverter}}" />
    </Grid>
</Window>
```

**Key Principles:**
- **Always use async/await** - Never use `.Result` or `.Wait()` (causes deadlocks)
- **UI thread affinity** - ViewModels have UI thread affinity, await automatically marshals back
- **ConfigureAwait in services** - Services should use `.ConfigureAwait(false)` to avoid UI thread
- **CancellationToken support** - Pass tokens through the call chain for cancellation
- **Progress reporting** - Use IProgress<T> for long-running operations
- **Handle all exceptions** - Wrap async operations in try/catch

**Service Layer Example:**

```csharp
public class TfsService
{
    public async Task<List<WorkItem>> GetWorkItemsAsync(CancellationToken ct = default)
    {
        // Service layer uses ConfigureAwait(false) - it's UI-agnostic
        await Task.Delay(TimeSpan.FromSeconds(1), ct).ConfigureAwait(false);
        
        var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<List<WorkItem>>(content);
    }
}
```

---

## 3. Timer Implementation for Auto-Refresh

### Decision
**Use DispatcherTimer for auto-refresh in WPF MVVM applications**

### Rationale
- **UI thread affinity**: Automatically runs on UI thread, safe for updating UI-bound properties
- **Simple and safe**: No need for manual Dispatcher.Invoke calls
- **WPF integration**: Designed specifically for WPF applications
- **Priority support**: Allows setting dispatcher priority for timer callbacks
- **Easier debugging**: Exceptions are raised on UI thread, easier to catch and debug

### Alternatives Considered

**System.Timers.Timer**
- Pros: Higher precision, runs on thread pool
- Cons: Elapsed event runs on thread pool thread, requires Dispatcher.Invoke for UI updates
- Verdict: Avoid for MVVM ViewModels with UI-bound properties
- Use case: Background services without UI interaction

**System.Threading.Timer**
- Pros: Lightweight, precise
- Cons: Same thread marshalling issues as System.Timers.Timer
- Verdict: Avoid for UI-bound scenarios

**Periodic Timer (.NET 6+)**
- Pros: Modern async API, efficient
- Cons: Still requires manual UI thread marshalling
- Verdict: Good for background services, not ideal for ViewModels

### Implementation Notes

**Pattern 1: DispatcherTimer in ViewModel**

```csharp
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _refreshTimer;
    private readonly ILogger<MainViewModel> _logger;
    
    public MainViewModel(ILogger<MainViewModel> logger)
    {
        _logger = logger;
        
        // Initialize timer
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };
        _refreshTimer.Tick += RefreshTimer_Tick;
    }

    [ObservableProperty]
    private bool _autoRefreshEnabled = true;

    partial void OnAutoRefreshEnabledChanged(bool value)
    {
        if (value)
        {
            _refreshTimer.Start();
            _logger.LogInformation("Auto-refresh enabled");
        }
        else
        {
            _refreshTimer.Stop();
            _logger.LogInformation("Auto-refresh disabled");
        }
    }

    private async void RefreshTimer_Tick(object sender, EventArgs e)
    {
        _logger.LogInformation("Auto-refresh triggered");
        
        // Async operation is safe here - we're already on UI thread
        await RefreshDataAsync();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        try
        {
            // Your refresh logic
            var items = await _tfsService.GetWorkItemsAsync();
            WorkItems.Clear();
            foreach (var item in items)
            {
                WorkItems.Add(item);
            }
            
            LastRefreshTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during refresh");
            StatusMessage = $"Refresh failed: {ex.Message}";
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Tick -= RefreshTimer_Tick;
    }
}
```

**Pattern 2: Configurable Refresh Interval**

```csharp
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _refreshTimer;
    
    [ObservableProperty]
    private int _refreshIntervalMinutes = 5;

    partial void OnRefreshIntervalMinutesChanged(int value)
    {
        if (value < 1) value = 1; // Minimum 1 minute
        if (value > 60) value = 60; // Maximum 60 minutes
        
        _refreshTimer.Interval = TimeSpan.FromMinutes(value);
        _logger.LogInformation($"Refresh interval changed to {value} minutes");
    }
}
```

**Pattern 3: Pause During User Interaction**

```csharp
public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    private async Task EditWorkItemAsync(WorkItem item)
    {
        // Pause timer during edit
        _refreshTimer.Stop();
        
        try
        {
            await ShowEditDialogAsync(item);
        }
        finally
        {
            // Resume timer after edit
            if (AutoRefreshEnabled)
            {
                _refreshTimer.Start();
            }
        }
    }
}
```

**XAML Binding Example:**

```xml
<StackPanel Orientation="Horizontal">
    <CheckBox Content="Auto-refresh every"
              IsChecked="{Binding AutoRefreshEnabled}" />
    <TextBox Text="{Binding RefreshIntervalMinutes, UpdateSourceTrigger=PropertyChanged}"
             Width="50"
             Margin="5,0" />
    <TextBlock Text="minutes" />
    <TextBlock Text="{Binding LastRefreshTime, StringFormat='Last refresh: {0:T}'}"
               Margin="20,0,0,0" />
</StackPanel>
```

**Best Practices:**
- **Dispose properly**: Stop timer and unsubscribe from Tick event in Dispose
- **Handle exceptions**: Wrap timer callback logic in try/catch
- **User control**: Allow users to enable/disable and configure interval
- **Pause during operations**: Stop timer during user editing or critical operations
- **Priority consideration**: Use `DispatcherPriority.Background` or `SystemIdle` if refresh is low priority
- **Avoid too frequent**: Minimum interval of 1 minute recommended for network calls

**Advanced: Priority-Based Timer**

```csharp
// Use lower priority to prevent interrupting user interactions
_refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
{
    Interval = TimeSpan.FromMinutes(5)
};
```

---

## 4. Error Handling and Dialogs in MVVM

### Decision
**Use service-based dialog abstraction with dependency injection to maintain separation of concerns**

### Rationale
- **Testability**: ViewModels remain testable without actual UI dependencies
- **Separation of concerns**: ViewModels don't directly reference UI types (MessageBox, etc.)
- **Flexibility**: Easy to swap implementations (e.g., testing vs. production)
- **Platform independence**: Dialog abstraction can be implemented differently per platform
- **SOLID principles**: Follows Dependency Inversion Principle

### Alternatives Considered

**Direct MessageBox calls**
- Cons: Tight coupling to UI, not testable, violates MVVM pattern
- Verdict: Avoid

**Messenger/Event aggregator**
- Pros: Loosely coupled
- Cons: Indirect, harder to track flow, no return values from dialogs
- Verdict: Acceptable but less intuitive than service abstraction

**Attached behaviors**
- Pros: Pure XAML approach
- Cons: Complex, limited functionality
- Verdict: Use for simple scenarios only

### Implementation Notes

**Pattern 1: Dialog Service Interface**

```csharp
public interface IDialogService
{
    void ShowError(string message, string title = "Error");
    void ShowWarning(string message, string title = "Warning");
    void ShowInfo(string message, string title = "Information");
    bool ShowConfirmation(string message, string title = "Confirm");
    Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");
}
```

**Pattern 2: WPF Dialog Service Implementation**

```csharp
public class WpfDialogService : IDialogService
{
    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowInfo(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool ShowConfirmation(string message, string title = "Confirm")
    {
        var result = MessageBox.Show(message, title, 
                                     MessageBoxButton.YesNo, 
                                     MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public Task<bool> ShowConfirmationAsync(string message, string title = "Confirm")
    {
        // MessageBox.Show is synchronous, but we wrap for async pattern
        return Task.FromResult(ShowConfirmation(message, title));
    }
}
```

**Pattern 3: Test Dialog Service**

```csharp
public class TestDialogService : IDialogService
{
    // For unit testing - can configure responses
    public bool ConfirmationResult { get; set; } = true;
    public List<string> MessagesShown { get; } = new();

    public void ShowError(string message, string title = "Error")
    {
        MessagesShown.Add($"Error: {message}");
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        MessagesShown.Add($"Warning: {message}");
    }

    public void ShowInfo(string message, string title = "Information")
    {
        MessagesShown.Add($"Info: {message}");
    }

    public bool ShowConfirmation(string message, string title = "Confirm")
    {
        MessagesShown.Add($"Confirm: {message}");
        return ConfirmationResult;
    }

    public Task<bool> ShowConfirmationAsync(string message, string title = "Confirm")
    {
        return Task.FromResult(ShowConfirmation(message, title));
    }
}
```

**Pattern 4: ViewModel Usage**

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly ITfsService _tfsService;
    private readonly ILogger<MainViewModel> _logger;

    public MainViewModel(
        IDialogService dialogService,
        ITfsService tfsService,
        ILogger<MainViewModel> logger)
    {
        _dialogService = dialogService;
        _tfsService = tfsService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task DeleteWorkItemAsync(WorkItem item)
    {
        var confirmed = _dialogService.ShowConfirmation(
            $"Are you sure you want to delete work item {item.Id}?",
            "Confirm Delete");

        if (!confirmed) return;

        try
        {
            await _tfsService.DeleteWorkItemAsync(item.Id);
            WorkItems.Remove(item);
            _dialogService.ShowInfo("Work item deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete work item {Id}", item.Id);
            _dialogService.ShowError($"Failed to delete work item: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            var items = await _tfsService.GetWorkItemsAsync();
            WorkItems = new ObservableCollection<WorkItem>(items);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during refresh");
            _dialogService.ShowError(
                "Unable to connect to TFS server. Please check your network connection.",
                "Connection Error");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Authentication error");
            _dialogService.ShowError(
                "Authentication failed. Please check your credentials.",
                "Authentication Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during refresh");
            _dialogService.ShowError(
                $"An unexpected error occurred: {ex.Message}",
                "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

**Pattern 5: Dependency Injection Setup**

```csharp
// In App.xaml.cs or Startup
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<IDialogService, WpfDialogService>();
        services.AddSingleton<ITfsService, TfsService>();
        services.AddSingleton<ILogger<MainViewModel>, Logger<MainViewModel>>();
        
        // Register ViewModels
        services.AddTransient<MainViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var mainWindow = new MainWindow
        {
            DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
        };
        
        mainWindow.Show();
    }
}
```

**Best Practices:**
- **Async dialogs**: Prefer async methods for future flexibility
- **Specific exceptions**: Catch specific exception types for better error messages
- **Logging**: Always log errors before showing to user
- **User-friendly messages**: Translate technical errors to user-friendly language
- **Contextual titles**: Use descriptive titles for dialogs
- **Testing**: Use test implementation to verify dialog interactions in unit tests

---

## 5. Visual Studio Detection

### Decision
**Use vswhere.exe (bundled with VS 2017+) via programmatic API for reliable VS detection**

### Rationale
- **Official Microsoft tool**: Maintained by Visual Studio team
- **Reliable**: Handles all VS editions, versions, and installation types
- **JSON output**: Easy to parse programmatically
- **Instance information**: Provides detailed info including install path, version, edition
- **Always available**: Bundled with VS 2017+ at known location
- **Supports all scenarios**: Handles side-by-side installations, preview versions, etc.

### Alternatives Considered

**Registry Detection**
- Pros: Direct, no external dependencies
- Cons: Registry layout changed with VS 2017, complex, doesn't handle all scenarios
- Verdict: Unreliable for VS 2017+

**Environment Variables**
- Pros: Simple
- Cons: Not always set, varies by installation
- Verdict: Insufficient

**WMI Queries**
- Pros: Standardized Windows interface
- Cons: Requires Visual Studio Client Detector Utility, less reliable
- Verdict: Secondary option

**Setup Configuration API**
- Pros: Programmatic COM API
- Cons: More complex than vswhere, requires COM interop
- Verdict: vswhere wraps this, easier to use

### Implementation Notes

**Pattern 1: Visual Studio Locator Service**

```csharp
public interface IVisualStudioLocator
{
    VisualStudioInstance FindLatestInstance();
    IEnumerable<VisualStudioInstance> FindAllInstances();
    bool IsVisualStudioInstalled();
}

public class VisualStudioInstance
{
    public string InstallationPath { get; set; }
    public string DisplayName { get; set; }
    public string Version { get; set; }
    public string ProductPath { get; set; } // Path to devenv.exe
}
```

**Pattern 2: vswhere Implementation**

```csharp
using System.Diagnostics;
using System.Text.Json;

public class VsWhereLocator : IVisualStudioLocator
{
    private const string VsWherePath = 
        @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";

    public bool IsVisualStudioInstalled()
    {
        return FindLatestInstance() != null;
    }

    public VisualStudioInstance FindLatestInstance()
    {
        var instances = FindAllInstances();
        return instances.OrderByDescending(i => i.Version).FirstOrDefault();
    }

    public IEnumerable<VisualStudioInstance> FindAllInstances()
    {
        if (!File.Exists(VsWherePath))
        {
            return Enumerable.Empty<VisualStudioInstance>();
        }

        try
        {
            var output = RunVsWhere("-latest -format json");
            var instances = ParseVsWhereJson(output);
            return instances;
        }
        catch (Exception ex)
        {
            // Log error
            return Enumerable.Empty<VisualStudioInstance>();
        }
    }

    private string RunVsWhere(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = VsWherePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"vswhere exited with code {process.ExitCode}");
        }

        return output;
    }

    private IEnumerable<VisualStudioInstance> ParseVsWhereJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Enumerable.Empty<VisualStudioInstance>();
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var instances = new List<VisualStudioInstance>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                instances.Add(ParseInstance(element));
            }
        }

        return instances;
    }

    private VisualStudioInstance ParseInstance(JsonElement element)
    {
        var installPath = element.GetProperty("installationPath").GetString();
        
        return new VisualStudioInstance
        {
            InstallationPath = installPath,
            DisplayName = element.GetProperty("displayName").GetString(),
            Version = element.GetProperty("installationVersion").GetString(),
            ProductPath = Path.Combine(installPath, "Common7", "IDE", "devenv.exe")
        };
    }
}
```

**Pattern 3: Usage in Application**

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IVisualStudioLocator _vsLocator;

    [RelayCommand]
    private void OpenInVisualStudio()
    {
        var vsInstance = _vsLocator.FindLatestInstance();

        if (vsInstance == null)
        {
            _dialogService.ShowWarning(
                "Visual Studio is not installed on this machine.",
                "Visual Studio Not Found");
            return;
        }

        try
        {
            // Launch VS with solution file
            var solutionPath = GetCurrentSolutionPath();
            
            var startInfo = new ProcessStartInfo
            {
                FileName = vsInstance.ProductPath,
                Arguments = $"\"{solutionPath}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch Visual Studio");
            _dialogService.ShowError(
                $"Failed to open Visual Studio: {ex.Message}",
                "Error");
        }
    }
}
```

**Best Practices:**
- **Error handling**: Handle cases where vswhere.exe doesn't exist
- **Logging**: Log detection attempts for troubleshooting
- **Caching**: Cache results if called frequently
- **User notification**: Inform user if VS is not found
- **Version checking**: Support different VS versions appropriately
- **Testing**: Mock IVisualStudioLocator for unit tests

---

## Summary and Recommendations

### Technology Stack
- **MVVM Framework**: CommunityToolkit.Mvvm (Microsoft MVVM Toolkit)
- **Async Pattern**: async/await with CancellationToken support
- **Timer**: DispatcherTimer for UI-safe auto-refresh
- **Error Handling**: IDialogService abstraction with DI
- **VS Detection**: vswhere.exe via programmatic wrapper

### Key Architectural Decisions

1. **Use dependency injection** throughout the application
2. **Maintain clear separation** between ViewModel (UI-affine) and Services (UI-agnostic)
3. **Leverage source generators** from CommunityToolkit.Mvvm to reduce boilerplate
4. **Implement proper async patterns** with cancellation support
5. **Abstract all UI interactions** (dialogs, etc.) behind interfaces

### Critical Implementation Points

- **Always use async/await** - never block with `.Result` or `.Wait()`
- **ViewModels have UI thread affinity** - don't use ConfigureAwait(false)
- **Services are UI-agnostic** - always use ConfigureAwait(false)
- **Dispose timers properly** - stop and unsubscribe in Dispose
- **Handle all exceptions** - catch, log, and show user-friendly messages
- **Use CancellationToken** for all async operations that can be cancelled

### Testing Strategy

- **Unit test ViewModels** using mock services (TestDialogService, etc.)
- **Integration test** with real services in isolated environment
- **UI test** critical user flows with automated UI testing framework

### Next Steps

1. Set up project structure with proper DI container
2. Install CommunityToolkit.Mvvm package
3. Implement core service interfaces (ITfsService, IDialogService, etc.)
4. Create base ViewModel infrastructure with async patterns
5. Implement timer-based auto-refresh with user controls
6. Add Visual Studio detection service
7. Write unit tests for ViewModels and services

---

**References:**
- [CommunityToolkit.Mvvm Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [WPF Threading Model](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/threading-model)
- [Async MVVM Data Binding Patterns (Stephen Cleary)](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/march/async-programming-patterns-for-asynchronous-mvvm-applications-data-binding)
- [vswhere Documentation](https://github.com/microsoft/vswhere)
- [Visual Studio Instance Detection](https://learn.microsoft.com/en-us/visualstudio/install/tools-for-managing-visual-studio-instances)
