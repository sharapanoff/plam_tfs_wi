# TFS Read-Only Viewer

A Windows desktop application for viewing Team Foundation Server (TFS) work items, pull requests, and code reviews assigned to you.

## 🌟Note About This Application

This application was built **entirely using Artificial Intelligence**.  
Its development methodology is **Spec-Driven Development**, powered by **spec-kit**.  
🔗 <https://github.com/github/spec-kit>

The idea was inspired by this insightful article:  
🔗 <https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html>


### 🎯 Purpose

I work on multiple projects simultaneously, and I needed a tool that allows me to quickly see **what tasks I currently have on my plate**.  
This app solves exactly that.


### 🛠 Development Process

While the tool generated most of the code automatically, some improvements were still necessary.  
My role was mostly **debugging**, and whenever I noticed needed changes, I tried to **express them as new specifications and requirements for the tool**, so it could update the code accordingly.

---


## Features

- 📋 **View Assigned Work Items** - See all work items assigned to you with ID, title, type, state, and assigned date
- 🔄 **View Pull Requests** - Track pull requests where you're a reviewer with details and status
- 📝 **View Code Reviews** - Monitor code reviews assigned to you (TFVC)
- 🔄 **Auto-Refresh** - Automatically refreshes data every 5 minutes
- 🚀 **Quick Launch** - Open items directly in Visual Studio 2022 or your default browser
- 💾 **Smart Caching** - Caches data for 5 minutes for fast performance
- 🔒 **Secure Credentials** - Stores TFS credentials securely in Windows Credential Manager
- 📊 **Material Design UI** - Modern, clean interface with Material Design

## Quick Start

### Prerequisites

- **Windows 10/11** (build 17763 or later)
- **.NET 6.0 or .NET 8.0** Runtime
- **TFS Server or Azure DevOps Server** access with a Personal Access Token (PAT)
- **Visual Studio 2022** (optional, for "Open in VS" feature)

### Installation

1. Download the latest release from the Releases page
2. Extract the ZIP file to your preferred location
3. Run `TfsViewer.App.exe`

### First-Time Setup

1. On first launch, you'll be prompted for TFS connection settings:
   - **Server URL**: Your TFS server URL (e.g., `https://tfs.company.com/DefaultCollection`)
   - **Authentication**: Choose Personal Access Token (recommended)
   - **Personal Access Token**: Generate from TFS with these scopes:
     - Work Items: Read
     - Code: Read
     - Pull Request Threads: Read

2. Click **Connect** to save credentials and test connection

3. The main window will load, showing three tabs:
   - **Work Items** - Assigned work items
   - **Pull Requests** - Pull requests where you're a reviewer
   - **Code Reviews** - Code reviews assigned to you

### Generating a Personal Access Token (PAT)

1. Navigate to your TFS/Azure DevOps Server in browser
2. Click on your profile icon → **Security** → **Personal Access Tokens**
3. Click **New Token**
4. Set scopes:
   - ✅ Work Items (Read)
   - ✅ Code (Read)
   - ✅ Pull Request Threads (Read)
5. Click **Create** and copy the token
6. Paste into TFS Viewer connection settings

## Usage

### Viewing Work Items

- Click the **Work Items** tab to see all work items assigned to you
- Each work item shows: ID, Title, Type, State, Assigned Date
- Click **Open in Browser** to view in TFS web interface
- Click **Open in VS** to open in Visual Studio 2022

### Viewing Pull Requests

- Click the **Pull Requests** tab to see PRs where you're a reviewer
- Each PR shows: ID, Title, Author, Created Date, Status
- Click **Open in Browser** to view in TFS web interface
- Click **Open in VS** to open in Visual Studio 2022

### Viewing Code Reviews

- Click the **Code Reviews** tab to see code reviews assigned to you
- Each review shows: ID, Title, Requester, Created Date, Status
- Click **Open in Browser** to view in TFS web interface
- Click **Open in VS** to open in Visual Studio 2022

### Refreshing Data

- Click the **Refresh** button in the toolbar to manually refresh all tabs
- Auto-refresh runs every 5 minutes automatically
- Click **Cancel** in any tab to stop an in-progress load operation

### Keyboard Shortcuts

- **F5** - Refresh all data
- **Ctrl+S** - Open Settings
- **Ctrl+W** - Exit application

## Configuration

### Settings File

Settings are stored in: `%LOCALAPPDATA%\TfsViewer\appsettings.json`

```json
{
  "TfsViewer": {
    "ServerUrl": "https://tfs.company.com/DefaultCollection",
    "CacheDuration": "00:05:00",
    "AutoRefreshInterval": "00:05:00"
  }
}
```

### Logs

Error logs are written to: `%LOCALAPPDATA%\TfsViewer\logs\app-YYYYMMDD.log`

Logs are retained for 14 days and include only warnings and errors.

## Troubleshooting

### "Cannot connect to TFS server"

1. Verify your TFS server URL is correct
2. Check your network connection
3. Verify your Personal Access Token is valid and has correct scopes
4. Check firewall settings (TFS usually uses port 443/HTTPS)

### "Visual Studio not detected"

1. Ensure Visual Studio 2022 is installed
2. Check registry key: `HKLM\SOFTWARE\Microsoft\VisualStudio\17.0`
3. Falls back to browser if VS not found

### Items not updating after refresh

1. Check TFS server is online
2. Verify your credentials haven't expired
3. Check logs in `%LOCALAPPDATA%\TfsViewer\logs\`
4. Try disconnecting and reconnecting in Settings

### Performance issues with many items

The app is optimized for:
- <2 seconds initial load
- <5 seconds refresh
- <100MB memory usage

If performance is slow:
1. Check your network connection to TFS
2. Reduce the number of assigned work items/PRs
3. Restart the application to clear cache

## Building from Source

### Prerequisites

- Visual Studio 2022 with .NET Desktop Development workload
- .NET 6.0 SDK or .NET 8.0 SDK
- Git

### Build Steps

```powershell
# Clone repository
git clone <repository-url>
cd plam_tfs_wi

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project src/TfsViewer.App/TfsViewer.App.csproj
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Architecture

- **UI Framework**: WPF with Material Design In XAML Toolkit
- **Pattern**: MVVM (Model-View-ViewModel) using CommunityToolkit.Mvvm
- **TFS Client**: Microsoft.TeamFoundationServer.Client
- **Caching**: System.Runtime.Caching (5-minute TTL)
- **Logging**: Serilog with file sink
- **Resilience**: Polly retry policies (3 retries with exponential backoff)
- **Credentials**: Windows Credential Manager

## Project Structure

```
src/
├── TfsViewer.App/          # WPF application
│   ├── Views/              # XAML views
│   ├── ViewModels/         # MVVM ViewModels
│   └── Services/           # UI services (launcher)
└── TfsViewer.Core/         # Core business logic
    ├── Api/                # TFS API client
    ├── Models/             # Domain models
    ├── Services/           # Core services (TFS, cache, logging)
    └── Contracts/          # Interfaces
```

## Contributing

See [specs/001-tfs-viewer/quickstart.md](specs/001-tfs-viewer/quickstart.md) for detailed development guide.

## License

[Your License Here]

## Support

For issues, questions, or feature requests, please:
- Check the logs at `%LOCALAPPDATA%\TfsViewer\logs\`
- Review [specs/001-tfs-viewer/spec.md](specs/001-tfs-viewer/spec.md) for requirements
- Consult [specs/001-tfs-viewer/quickstart.md](specs/001-tfs-viewer/quickstart.md) for development guide

## Acknowledgments

- Material Design In XAML Toolkit - https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit
- CommunityToolkit.Mvvm - https://github.com/CommunityToolkit/dotnet
- Microsoft TFS Client Libraries - https://www.nuget.org/packages/Microsoft.TeamFoundationServer.Client/
