# SharpReady - C# MAUI Android Application

A modern, production-ready Android application built with .NET MAUI (Multi-platform App UI). This project demonstrates best practices for mobile development using C#, including MVVM architecture, dependency injection, and modular design.

## Technology Stack

- **.NET 9.0**: Latest long-term support framework
- **.NET MAUI**: Microsoft's unified multi-platform UI framework
- **C# 12**: Modern language features with nullable reference types
- **XAML**: Declarative UI markup language
- **Dependency Injection**: Built-in Microsoft.Extensions.DependencyInjection

## Project Architecture

### Folder Structure

```
SharpReady/
├── Models/                 # Data models and entities
│   ├── BaseModel.cs       # Base class for all models
│   └── UserProfile.cs     # Example user profile model
├── Services/              # Business logic and data access
│   ├── IDataService.cs    # Data persistence interface
│   ├── MockDataService.cs # In-memory data service
│   ├── INavigationService.cs  # Navigation interface
│   └── NavigationService.cs   # Navigation implementation
├── ViewModels/            # MVVM ViewModels
│   └── MainViewModel.cs   # Main page ViewModel
├── Views/                 # XAML pages and UI
│   ├── MainPage.xaml      # Home page
│   ├── MainPage.xaml.cs   # Home code-behind
│   └── ProfilePage.xaml   # Profile page
├── Utilities/             # Helper classes and extensions
│   └── BaseViewModel.cs   # Base ViewModel with INotifyPropertyChanged
├── Resources/             # App resources (fonts, images, styles)
├── Platforms/             # Platform-specific code
│   └── Android/          # Android-specific implementations
├── App.xaml               # Application-level resources
├── AppShell.xaml          # Shell navigation structure
└── MauiProgram.cs         # DI container configuration

```

## Key Features

### MVVM Architecture
- **BaseViewModel**: Abstract base class implementing `INotifyPropertyChanged`
- **SetProperty Pattern**: Type-safe property binding with change notification
- **Data Binding**: Two-way binding for reactive UI updates

### Dependency Injection
Services registered in `MauiProgram.cs`:
- `IDataService`: Data persistence (implement with SQLite, REST API, etc.)
- `INavigationService`: Page routing and navigation
- `MainViewModel`: Main page business logic

### Data Models
- `BaseModel`: Abstract base with common properties (Id, CreatedAt, UpdatedAt, Notes)
- `UserProfile`: Example entity demonstrating model patterns

### Services
- **MockDataService**: In-memory implementation for development/testing
- **NavigationService**: Wrapper around MAUI Shell for type-safe routing

## Getting Started

### Prerequisites
- .NET 9 SDK or later
- Android SDK (for emulator/device debugging)
- Visual Studio 2022+ or Visual Studio Code with C# Dev Kit
- Android Studio (recommended for emulator)

### Building the Project

```bash
cd D:\Code\claude\SharpReady

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Build Android-specific
dotnet build -f net9.0-android
```

### Running on Android Emulator

```bash
# Run on default Android emulator
dotnet run -f net9.0-android

# Or with specific device
dotnet run -f net9.0-android --no-build
```

### Running on Physical Device

```bash
# Connect your Android device with USB debugging enabled
dotnet run -f net9.0-android
```

## Development Guidelines

### Adding a New Model

1. Create a new class in `Models/` inheriting from `BaseModel`
2. Add data annotations for validation
3. Register with the data service

```csharp
// Example: Models/Task.cs
public class Task : BaseModel
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
```

### Adding a New Service

1. Create an interface in `Services/IMyService.cs`
2. Implement the interface in `Services/MyService.cs`
3. Register in `MauiProgram.cs`:

```csharp
private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
{
    builder.Services.AddSingleton<IMyService, MyService>();
    return builder;
}
```

### Adding a New Page

1. Create XAML file in `Views/MyPage.xaml`
2. Create code-behind in `Views/MyPage.xaml.cs`
3. Create ViewModel in `ViewModels/MyPageViewModel.cs`
4. Register in `MauiProgram.cs`
5. Add route to `AppShell.xaml`

```csharp
// MyPageViewModel.cs
public class MyPageViewModel : BaseViewModel
{
    // Implementation
}

// MyPage.xaml.cs
public partial class MyPage : ContentPage
{
    public MyPage(MyPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

## Configuration

### Android Manifest
Located in `Platforms/Android/AndroidManifest.xml`:
- Define required permissions
- Configure application metadata
- Set up intent filters

### App Configuration
Update in `SharpReady.csproj`:
- `ApplicationId`: Package identifier (e.g., `com.example.myapp`)
- `ApplicationDisplayVersion`: User-facing version (e.g., `1.0`)
- `ApplicationVersion`: Internal version number
- `SupportedOSPlatformVersion`: Minimum Android API level (currently 21)

## Data Persistence

Currently uses `MockDataService` for development. For production:

### Option 1: SQLite with Entity Framework Core
```csharp
// Add to MauiProgram.cs
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddSingleton<IDataService, SqliteDataService>();
```

### Option 2: REST API
```csharp
// Create ApiDataService inheriting from IDataService
// Implement HTTP calls to backend API
```

### Option 3: Azure Cosmos DB / Firebase
```csharp
// Use official SDKs for cloud database
```

## Best Practices

1. **Property Binding**: Always use `SetProperty()` in ViewModels
2. **Async/Await**: Keep UI responsive with async operations
3. **Error Handling**: Wrap data operations in try-catch blocks
4. **Nullable Reference Types**: Use `#nullable enable` for type safety
5. **Command Binding**: Use `Command` for button clicks in MVVM
6. **Resource Management**: Dispose of resources properly

## Platform-Specific Considerations

### Android-Specific Code
Location: `Platforms/Android/`

Features:
- `MainActivity`: Entry point for the Android app
- `MainApplication`: Application class with lifecycle methods
- `AndroidManifest.xml`: App configuration and permissions

To add Android-specific functionality:
```csharp
#if ANDROID
using Android.App;
using Android.Content;

// Android-specific implementation
#endif
```

## Testing

### Unit Testing Structure (to be added)
```
SharpReady.Tests/
├── Models/
├── Services/
└── ViewModels/
```

## Troubleshooting

### Build Issues
- Clean NuGet cache: `dotnet nuget locals all --clear`
- Clear build: `dotnet clean && dotnet build`

### Runtime Issues
- Check Android SDK versions match `AndroidManifest.xml`
- Verify emulator has sufficient storage
- Check LogCat output for detailed errors

## Resources

- [MAUI Documentation](https://learn.microsoft.com/dotnet/maui)
- [MVVM Pattern Guide](https://learn.microsoft.com/dotnet/maui/xaml/bindings/mvvm)
- [Async/Await Best Practices](https://learn.microsoft.com/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Android Developer Documentation](https://developer.android.com)

## Next Steps

1. Implement SQLite database layer
2. Add unit tests
3. Create custom controls and behaviors
4. Implement offline sync
5. Add analytics and crash reporting
6. Optimize performance and battery usage
7. Create production build configuration

## License

This project is provided as-is for educational and development purposes.
