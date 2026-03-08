# SharpReady Architecture Document

## Overview

This document describes the architectural decisions and patterns used in the SharpReady project.

## Design Patterns

### 1. MVVM (Model-View-ViewModel)

**Why MVVM?**
- Separates presentation logic from business logic
- Enables data binding and reactive UI updates
- Improves testability by isolating ViewModels
- Reduces code-behind logic in Views

**Implementation:**
- `BaseViewModel` provides `INotifyPropertyChanged` implementation
- `SetProperty<T>()` method handles change notification
- Views bind to ViewModel properties via XAML binding expressions

**Example:**
```csharp
// ViewModel
public class MainViewModel : BaseViewModel
{
    private int _count;

    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }
}

// View
<Label Text="{Binding Count}" />
```

### 2. Dependency Injection

**Why DI?**
- Loose coupling between components
- Easy to swap implementations (mock for tests, real for production)
- Centralized configuration in `MauiProgram.cs`

**Implementation:**
```csharp
// MauiProgram.cs
private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
{
    builder.Services.AddSingleton<IDataService, MockDataService>();
    builder.Services.AddSingleton<INavigationService, NavigationService>();
    return builder;
}
```

**Usage:**
```csharp
// Constructor injection
public MainViewModel(IDataService dataService)
{
    _dataService = dataService;
}
```

### 3. Repository Pattern (via IDataService)

**Why Repository?**
- Abstracts data access layer
- Single place to change persistence mechanism
- Testable through mocking

**Current Implementation:**
```csharp
public interface IDataService
{
    Task<T?> GetItemAsync<T>(int id) where T : class;
    Task<IEnumerable<T>> GetItemsAsync<T>() where T : class;
    Task<int> SaveItemAsync<T>(T item) where T : class;
    Task<bool> DeleteItemAsync<T>(int id) where T : class;
}
```

**Usage:**
```csharp
public class MainViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    public async Task LoadData()
    {
        var users = await _dataService.GetItemsAsync<UserProfile>();
    }
}
```

## Data Flow

```
User Action (Button Click)
    ↓
View (XAML)
    ↓
Command Handler / Event
    ↓
ViewModel (Business Logic)
    ↓
Service (IDataService, INavigationService)
    ↓
Data Source (SQLite, API, etc.)
    ↓
Response back through chain
    ↓
ViewModel updates properties via SetProperty()
    ↓
View updates automatically via binding
```

## Folder Organization Rationale

### Models/
- **Purpose**: Data contracts and entities
- **Naming**: Singular (e.g., `UserProfile`, not `Users`)
- **Responsibility**: Data structure and validation only
- **No Logic**: Business logic belongs in Services/ViewModels

### Services/
- **Purpose**: Business logic and data access
- **Pattern**: Interface + Implementation
- **Responsibility**: Orchestrate data flow, business rules
- **Testable**: Easy to mock via interfaces

### ViewModels/
- **Purpose**: Presentation logic and state management
- **Inherits From**: `BaseViewModel`
- **Responsibility**: Command handling, property exposure to Views
- **Data Binding**: All public properties trigger UI updates

### Views/
- **Purpose**: XAML UI and minimal code-behind
- **Naming**: `PageName.xaml` + `PageName.xaml.cs`
- **Responsibility**: UI layout and binding only
- **Code-Behind**: Constructor injection and minimal event handlers

### Utilities/
- **Purpose**: Shared helpers and extensions
- **Example**: `BaseViewModel`, custom behaviors, converters
- **Reusability**: Used across multiple Views/ViewModels

### Platforms/Android/
- **Purpose**: Android-specific code and configuration
- **Structure**: Mirrors Android/Kotlin project structure
- **Usage**: Activity customization, permissions, Android APIs

## Dependency Lifecycle

```
MauiProgram.cs (Startup)
    ↓
AddSingleton (Application lifetime)
    ↓
Service instances created once at app start
    ↓
Injected into ViewModels
    ↓
ViewModels used by Views
    ↓
Same instance reused throughout app lifetime
```

### Singleton vs Transient
- **Singleton** (Current): One instance for entire app (Data services)
- **Transient** (Future): New instance each time (if needed)

## Data Binding

### Two-Way Binding
```xaml
<Entry Text="{Binding UserName, Mode=TwoWay}" />
```
- User types → ViewModel property updates
- Code updates property → UI updates immediately

### One-Way Binding
```xaml
<Label Text="{Binding Title}" />
```
- Changes to Title update Label
- User can't edit Label directly

### Command Binding
```xaml
<Button Command="{Binding LoadDataCommand}" />
```
- Connects button click to ViewModel command
- No code-behind needed

## Async Patterns

### ICommand with async
```csharp
public ICommand LoadDataCommand { get; }

public MainViewModel()
{
    LoadDataCommand = new Command(async () => await LoadData());
}

private async Task LoadData()
{
    IsBusy = true;
    var data = await _dataService.GetItemsAsync<UserProfile>();
    IsBusy = false;
}
```

### Benefits
- Keeps UI responsive
- Progress can be shown via `IsBusy` binding
- Error handling via try-catch in async methods

## Future Enhancements

### 1. Database Layer
```
SQLite → Entity Framework Core → DbContext
                ↓
         IDataService implementation
```

### 2. API Integration
```
REST API → HttpClient → Service Layer
           ↓
    IDataService implementation with offline fallback
```

### 3. State Management
```
Redux/MVVM pattern + Service → Central state store
                    ↓
            Multiple ViewModels subscribe to changes
```

### 4. Testing Structure
```
Android.Tests/
├── Unit/
│   ├── ViewModels/
│   ├── Services/
│   └── Models/
└── Integration/
    └── Database/
```

## Performance Considerations

### 1. Lazy Loading
- Load data on-demand, not at app startup
- Use ObservableCollection for large lists

### 2. Caching
- Cache frequently accessed data
- Implement in IDataService

### 3. Threading
- Keep heavy operations off UI thread
- Use Task-based APIs

### 4. Memory Management
- Dispose resources properly
- Use weak event patterns where needed

## Security Considerations

### 1. Data Storage
- Don't store sensitive data in SharedPreferences
- Use Android KeyStore for encryption
- Implement secure local database

### 2. Network Communication
- Use HTTPS only
- Pin SSL certificates
- Validate API responses

### 3. Code Protection
- Implement code obfuscation for release builds
- Use SecureString for passwords
- Avoid logging sensitive data

## Configuration Management

### Debug vs Release
```xml
<!-- SharpReady.csproj -->
<PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <CustomProperty>DebugValue</CustomProperty>
</PropertyGroup>
```

### Platform Configuration
```xml
<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
    <SupportedOSPlatformVersion>21.0</SupportedOSPlatformVersion>
</PropertyGroup>
```

## Summary

The SharpReady architecture follows industry best practices:
- **MVVM** for clean separation of concerns
- **Dependency Injection** for loose coupling
- **Repository Pattern** for data abstraction
- **Async/Await** for responsive UI
- **Clear Folder Structure** for maintainability
- **Interface-Based Design** for testability

This foundation supports scaling from a simple app to a complex enterprise application.
