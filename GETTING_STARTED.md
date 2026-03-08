# Getting Started with SharpReady

## Quick Setup (5 minutes)

### 1. Prerequisites Check
```bash
# Verify .NET SDK
dotnet --version

# Should show version 9.0 or higher
```

### 2. Navigate to Project
```bash
cd D:\Code\claude\SharpReady
```

### 3. Restore Dependencies
```bash
dotnet restore
```

### 4. Build Project
```bash
# Build for current platform
dotnet build

# Or build specifically for Android
dotnet build -f net9.0-android
```

## Running the App

### Option 1: Android Emulator

**Setup Emulator (One-time)**
1. Open Android Studio
2. Click "Virtual Device Manager"
3. Create a new emulator (Pixel 6, Android 13+)
4. Start the emulator

**Run App**
```bash
# Run on emulator
dotnet run -f net9.0-android

# Or with no rebuild
dotnet run -f net9.0-android --no-build
```

### Option 2: Physical Android Device

1. Enable USB Debugging on device:
   - Settings → Developer Options → USB Debugging
2. Connect device via USB
3. Run:
```bash
dotnet run -f net9.0-android
```

### Option 3: Visual Studio
1. Open `SharpReady.csproj` in Visual Studio 2022
2. Select "Android Emulator" or your device from the dropdown
3. Press F5 to debug

## Understanding the Project Structure

```
SharpReady/
├── Views/               ← UI Pages (XAML)
│   ├── MainPage.xaml   ← Home screen
│   └── ProfilePage.xaml ← Profile screen
├── ViewModels/         ← Business Logic (C#)
│   └── MainViewModel.cs ← Handles main page logic
├── Models/             ← Data Entities
│   ├── BaseModel.cs    ← Common properties
│   └── UserProfile.cs  ← Example entity
├── Services/           ← Data & Navigation
│   ├── IDataService.cs ← Data interface
│   └── MockDataService.cs ← Test implementation
├── Utilities/          ← Helper Classes
│   └── BaseViewModel.cs ← Base ViewModel
└── Platforms/Android/  ← Android-specific code
```

## Making Your First Change

### Change the Welcome Message

**File**: `D:\Code\claude\SharpReady\ViewModels\MainViewModel.cs`

Find this line:
```csharp
private string _title = "Welcome to SharpReady";
```

Change to:
```csharp
private string _title = "Welcome to My Android App";
```

Rebuild and run:
```bash
dotnet build && dotnet run -f net9.0-android
```

### Change Colors

**File**: `D:\Code\claude\SharpReady\MainPage.xaml`

Find:
```xaml
<Button
    Text="Increment Counter"
    ...
    BackgroundColor="#007AFF" />
```

Change `#007AFF` to another hex color:
- `#34C759` - Green
- `#FF3B30` - Red
- `#FF9500` - Orange

## Understanding Data Flow

### Counter Button Example

**User clicks "Increment Counter"**

```
Button Click (MainPage.xaml)
    ↓
OnCounterClicked() (MainPage.xaml.cs)
    ↓
viewModel.IncrementCounter() (MainViewModel.cs)
    ↓
Count property increases: SetProperty(ref _count, value)
    ↓
StatusMessage updates
    ↓
{Binding Count} in XAML updates automatically ✓
```

### Load Data Button Example

**User clicks "Load Data"**

```
Button.Command (MainPage.xaml)
    ↓
LoadDataCommand = new Command(async () => await LoadData())
    ↓
LoadData() async method
    ↓
IsBusy = true (shows loading state)
    ↓
dataService.GetItemsAsync<UserProfile>()
    ↓
MockDataService returns test data
    ↓
Users collection updated
    ↓
IsBusy = false (loading complete)
    ↓
UI updates automatically ✓
```

## Common Tasks

### Add a New Page

1. **Create XAML file**: `Views/SettingsPage.xaml`
```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SharpReady.Views.SettingsPage"
             Title="Settings">

    <VerticalStackLayout Padding="20">
        <Label Text="Settings" FontSize="24" FontAttributes="Bold" />
    </VerticalStackLayout>

</ContentPage>
```

2. **Create Code-Behind**: `Views/SettingsPage.xaml.cs`
```csharp
using SharpReady.ViewModels;

namespace SharpReady.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

3. **Create ViewModel**: `ViewModels/SettingsViewModel.cs`
```csharp
using SharpReady.Utilities;

namespace SharpReady.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _appVersion = "1.0.0";

    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }
}
```

4. **Register in MauiProgram.cs**
```csharp
private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
{
    builder.Services.AddSingleton<MainViewModel>();
    builder.Services.AddSingleton<SettingsViewModel>();  // Add this
    return builder;
}

private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
{
    builder.Services.AddSingleton<MainPage>();
    builder.Services.AddSingleton<SettingsPage>();  // Add this
    return builder;
}
```

5. **Add to AppShell.xaml**
```xaml
<ShellContent
    Title="Settings"
    ContentTemplate="{DataTemplate views:SettingsPage}"
    Route="SettingsPage" />
```

### Add a Model

**File**: `Models/Task.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace SharpReady.Models;

public class Task : BaseModel
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? DueDate { get; set; }
}
```

### Add a Service

**Interface**: `Services/ITaskService.cs`
```csharp
using SharpReady.Models;

namespace SharpReady.Services;

public interface ITaskService
{
    Task<List<Task>> GetTasksAsync();
    Task<Task?> GetTaskAsync(int id);
    Task SaveTaskAsync(Task task);
    Task DeleteTaskAsync(int id);
}
```

**Implementation**: `Services/TaskService.cs`
```csharp
using SharpReady.Models;
using SharpReady.Services;

namespace SharpReady.Services;

public class TaskService : ITaskService
{
    private readonly IDataService _dataService;

    public TaskService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<Task>> GetTasksAsync()
    {
        var tasks = await _dataService.GetItemsAsync<Task>();
        return tasks.ToList();
    }

    // Implement other methods...
}
```

**Register in MauiProgram.cs**
```csharp
builder.Services.AddSingleton<ITaskService, TaskService>();
```

## Debugging

### Enable Debug Logging

In `MauiProgram.cs`, debug logging is already enabled in DEBUG mode:
```csharp
#if DEBUG
    builder.Logging.AddDebug();
#endif
```

### View Debug Output

**Visual Studio**:
1. Debug → Windows → Output
2. Show output from: "Debug"

**Command Line**:
```bash
# Run with verbose output
dotnet run -f net9.0-android -v d
```

### Common Issues

**Issue**: "No suitable Android device found"
- Solution: Start Android emulator first, then run app

**Issue**: "MAUI dependencies not found"
- Solution: `dotnet restore && dotnet clean && dotnet build`

**Issue**: "Android SDK not found"
- Solution: Install via Android Studio → Tools → SDK Manager

## Next Steps

1. **Explore the Code**
   - Read through `MainViewModel.cs` to understand MVVM
   - Check `MainPage.xaml` to see data binding

2. **Make Changes**
   - Modify UI in XAML files
   - Update logic in ViewModel files
   - Test changes with emulator

3. **Add Features**
   - Create new pages
   - Add database persistence
   - Integrate with APIs

4. **Learn More**
   - Read `README.md` for architecture overview
   - Read `ARCHITECTURE.md` for design patterns
   - Visit [MAUI Docs](https://learn.microsoft.com/dotnet/maui)

## Quick Command Reference

```bash
# Build
dotnet build
dotnet build -f net9.0-android

# Run
dotnet run -f net9.0-android
dotnet run -f net9.0-android --no-build

# Clean
dotnet clean

# Restore
dotnet restore

# Package
dotnet publish -f net9.0-android -c Release
```

## IDE Recommendations

### Visual Studio 2022
- Best experience for MAUI development
- Built-in emulator management
- Full debugging support

### Visual Studio Code
- Install C# Dev Kit extension
- Install MAUI extension
- Good for lightweight development

### JetBrains Rider
- Excellent code completion
- Integrated terminal
- Good refactoring tools

Happy coding!
