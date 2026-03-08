# SharpReady - UI Implementation Patterns & Code Examples

This document provides concrete code examples and patterns for implementing the screens designed in the UI/UX plan.

---

## Table of Contents

1. [XAML Patterns](#xaml-patterns)
2. [Data Binding Examples](#data-binding-examples)
3. [Custom Controls](#custom-controls)
4. [Navigation Implementation](#navigation-implementation)
5. [Animation & Effects](#animation-effects)
6. [Responsive Design Code](#responsive-design-code)
7. [State Management Patterns](#state-management-patterns)

---

## XAML Patterns

### Pattern 1: Page Template with Sticky Bottom Bar

This pattern is used on Dashboard, Topic Details, and other pages with fixed bottom actions.

**File**: `Views/DashboardPage.xaml`

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SharpReady.Views.DashboardPage"
             Title="Home"
             BackgroundColor="{StaticResource WhiteColor}">

    <Grid RowDefinitions="*,Auto" RowSpacing="0">

        <!-- Scrollable Content -->
        <ScrollView Grid.Row="0">
            <VerticalStackLayout Padding="20,16" Spacing="16">

                <!-- Greeting Header -->
                <VerticalStackLayout Spacing="4">
                    <Label Text="{Binding GreetingMessage}"
                           FontSize="28"
                           FontAttributes="Bold"
                           TextColor="{StaticResource DarkGrayColor}" />
                    <Label Text="{Binding DateDisplay}"
                           FontSize="12"
                           TextColor="{StaticResource MediumGrayColor}" />
                </VerticalStackLayout>

                <!-- User Card -->
                <Frame BorderColor="{StaticResource LightGrayColor}"
                       CornerRadius="12"
                       Padding="16"
                       HasShadow="True">
                    <HorizontalStackLayout Spacing="12">
                        <Frame CornerRadius="30" Padding="0" HasShadow="False">
                            <Image Source="user_avatar.png"
                                   Aspect="AspectFill"
                                   WidthRequest="60"
                                   HeightRequest="60" />
                        </Frame>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Label Text="{Binding CurrentUser.FullName}"
                                   FontSize="16"
                                   FontAttributes="Bold" />
                            <HorizontalStackLayout>
                                <Label Text="★★★★★"
                                       TextColor="{StaticResource WarningColor}"
                                       FontSize="12" />
                                <Label Text="{Binding AverageRating, StringFormat='{0:0.0}'}"
                                       FontSize="12"
                                       TextColor="{StaticResource MediumGrayColor}" />
                            </HorizontalStackLayout>
                        </VerticalStackLayout>
                    </HorizontalStackLayout>
                </Frame>

                <!-- Daily Goal Progress -->
                <Label Text="Today's Goal"
                       FontSize="16"
                       FontAttributes="Bold"
                       TextColor="{StaticResource DarkGrayColor}" />

                <Frame BorderColor="{StaticResource LightGrayColor}"
                       CornerRadius="12"
                       Padding="16"
                       HasShadow="True">
                    <VerticalStackLayout Spacing="12">
                        <!-- Progress Ring (Custom Control) -->
                        <local:ProgressRingControl
                            Progress="{Binding DailyGoalProgress}"
                            Radius="80"
                            LineWidth="8"
                            ProgressColor="{StaticResource SuccessColor}"
                            BackgroundColor="{StaticResource LightGrayColor}"
                            HorizontalOptions="Center" />

                        <Grid ColumnDefinitions="*,*">
                            <Label Text="{Binding DailyGoalMinutes, StringFormat='{0} min'}"
                                   FontSize="14"
                                   HorizontalTextAlignment="Start" />
                            <Label Text="{Binding DailyGoalTarget, StringFormat='Target: {0} min'}"
                                   FontSize="14"
                                   HorizontalTextAlignment="End"
                                   TextColor="{StaticResource MediumGrayColor}" />
                        </Grid>
                    </VerticalStackLayout>
                </Frame>

                <!-- Quick Stats Grid -->
                <Label Text="Quick Stats"
                       FontSize="16"
                       FontAttributes="Bold"
                       TextColor="{StaticResource DarkGrayColor}" />

                <CollectionView ItemsSource="{Binding QuickStats}">
                    <CollectionView.ItemsLayout>
                        <GridItemsLayout Orientation="Vertical"
                                       HorizontalItemSpacing="12"
                                       VerticalItemSpacing="12"
                                       Rows="3" />
                    </CollectionView.ItemsLayout>
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <Frame BorderColor="{StaticResource LightGrayColor}"
                                   CornerRadius="12"
                                   Padding="16"
                                   HasShadow="True">
                                <VerticalStackLayout Spacing="8" HorizontalOptions="Center">
                                    <Label Text="{Binding Value}"
                                           FontSize="24"
                                           FontAttributes="Bold"
                                           TextColor="{StaticResource PrimaryColor}"
                                           HorizontalTextAlignment="Center" />
                                    <Label Text="{Binding Label}"
                                           FontSize="12"
                                           TextColor="{StaticResource MediumGrayColor}"
                                           HorizontalTextAlignment="Center" />
                                </VerticalStackLayout>
                            </Frame>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>

                <!-- Recent Activity -->
                <Label Text="Recent Activity"
                       FontSize="16"
                       FontAttributes="Bold"
                       TextColor="{StaticResource DarkGrayColor}" />

                <CollectionView ItemsSource="{Binding RecentActivity}">
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <Frame BorderColor="{StaticResource LightGrayColor}"
                                   CornerRadius="8"
                                   Padding="12"
                                   Margin="0,0,0,8"
                                   HasShadow="False">
                                <HorizontalStackLayout Spacing="12">
                                    <Label Text="{Binding Icon}"
                                           FontSize="20"
                                           VerticalOptions="Start"
                                           WidthRequest="30" />
                                    <VerticalStackLayout Spacing="2">
                                        <Label Text="{Binding Message}"
                                               FontSize="14"
                                               FontAttributes="Bold"
                                               TextColor="{StaticResource DarkGrayColor}" />
                                        <Label Text="{Binding Timestamp, StringFormat='{0:t}'}"
                                               FontSize="12"
                                               TextColor="{StaticResource MediumGrayColor}" />
                                    </VerticalStackLayout>
                                </HorizontalStackLayout>
                            </Frame>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>

            </VerticalStackLayout>
        </ScrollView>

        <!-- Sticky Bottom Action Bar -->
        <Grid Grid.Row="1"
              ColumnDefinitions="*,*"
              ColumnSpacing="12"
              Padding="16"
              BackgroundColor="{StaticResource WhiteColor}">

            <Button Text="Continue Study"
                    Grid.Column="0"
                    Style="{StaticResource PrimaryButtonStyle}"
                    Command="{Binding ContinueStudyCommand}" />

            <Button Text="New Study Plan"
                    Grid.Column="1"
                    Style="{StaticResource SecondaryButtonStyle}"
                    Command="{Binding CreateStudyPlanCommand}" />
        </Grid>

    </Grid>

</ContentPage>
```

---

### Pattern 2: List with Filtering and Search

**File**: `Views/StudyMaterialsPage.xaml`

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SharpReady.Views.StudyMaterialsPage"
             Title="Study Materials"
             BackgroundColor="{StaticResource WhiteColor}">

    <Grid RowDefinitions="Auto,Auto,*" RowSpacing="0">

        <!-- Search Bar -->
        <SearchBar Grid.Row="0"
                   Placeholder="Search topics..."
                   Text="{Binding SearchQuery}"
                   SearchCommand="{Binding SearchCommand}"
                   BackgroundColor="{StaticResource LightGrayColor}"
                   Margin="16,12"
                   CornerRadius="8" />

        <!-- Category Pills (Horizontal Scroll) -->
        <CollectionView Grid.Row="1"
                        ItemsSource="{Binding Categories}"
                        SelectionMode="Single"
                        SelectedItem="{Binding SelectedCategory}"
                        SelectionChangedCommand="{Binding CategorySelectedCommand}"
                        SelectionChangedCommandParameter="{Binding SelectedItem, Source={RelativeSource Self}}"
                        ScrollOrientation="Horizontal"
                        Margin="16,0,16,12">

            <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Horizontal"
                                   ItemSpacing="8" />
            </CollectionView.ItemsLayout>

            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame BorderColor="{Binding IsSelected, Converter={StaticResource BoolToColorConverter}, ConverterParameter='#007AFF,#D1D1D6'}"
                           CornerRadius="20"
                           Padding="16,8"
                           HasShadow="False"
                           BackgroundColor="{Binding IsSelected, Converter={StaticResource BoolToColorConverter}, ConverterParameter='#F2F2F7,#FFFFFF'}">
                        <Label Text="{Binding Name}"
                               FontSize="12"
                               FontAttributes="Bold"
                               TextColor="{Binding IsSelected, Converter={StaticResource BoolToColorConverter}, ConverterParameter='#007AFF,#333333'}" />
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Topics List -->
        <CollectionView Grid.Row="2"
                        ItemsSource="{Binding FilteredTopics}"
                        SelectionMode="Single"
                        SelectionChangedCommand="{Binding SelectTopicCommand}"
                        SelectionChangedCommandParameter="{Binding SelectedItem, Source={RelativeSource Self}}">

            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:StudyTopic">
                    <Frame BorderColor="{StaticResource LightGrayColor}"
                           CornerRadius="12"
                           Padding="16"
                           Margin="16,0,16,12"
                           HasShadow="True">

                        <VerticalStackLayout Spacing="8">
                            <!-- Title and Status -->
                            <Grid ColumnDefinitions="*,Auto">
                                <Label Text="{Binding Title}"
                                       FontSize="16"
                                       FontAttributes="Bold"
                                       TextColor="{StaticResource DarkGrayColor}" />

                                <Label Grid.Column="1"
                                       Text="{Binding IsCompleted, StringFormat='{0:C}'}"
                                       FontSize="18"
                                       TextColor="{StaticResource SuccessColor}">
                                    <Label.Text>
                                        <MultiBinding StringFormat="{0}">
                                            <Binding Path="IsCompleted" />
                                        </MultiBinding>
                                    </Label.Text>
                                </Label>
                            </Grid>

                            <!-- Metadata Row -->
                            <HorizontalStackLayout Spacing="16">
                                <HorizontalStackLayout Spacing="4">
                                    <Label Text="★"
                                           TextColor="{StaticResource WarningColor}"
                                           FontSize="12" />
                                    <Label Text="{Binding DifficultyLevel}"
                                           FontSize="12"
                                           TextColor="{StaticResource MediumGrayColor}" />
                                </HorizontalStackLayout>

                                <HorizontalStackLayout Spacing="4">
                                    <Label Text="⏱"
                                           FontSize="12" />
                                    <Label Text="{Binding EstimatedMinutes, StringFormat='{0} min'}"
                                           FontSize="12"
                                           TextColor="{StaticResource MediumGrayColor}" />
                                </HorizontalStackLayout>
                            </HorizontalStackLayout>

                            <!-- Progress Bar -->
                            <ProgressBar Progress="{Binding ProgressPercentage, Converter={StaticResource PercentageConverter}}"
                                         ProgressColor="{StaticResource SuccessColor}"
                                         BackgroundColor="{StaticResource LightGrayColor}"
                                         HeightRequest="8"
                                         CornerRadius="4" />

                            <Label Text="{Binding ProgressPercentage, StringFormat='{0:0}% Complete'}"
                                   FontSize="11"
                                   TextColor="{StaticResource MediumGrayColor}" />
                        </VerticalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

    </Grid>

</ContentPage>
```

---

## Data Binding Examples

### Two-Way Binding with Validation

```xaml
<!-- EditProfilePage.xaml -->
<Entry x:Name="NameEntry"
       Placeholder="Full Name"
       Text="{Binding CurrentUser.FullName, Mode=TwoWay}"
       BackgroundColor="{StaticResource LightGrayColor}"
       Padding="12"
       CornerRadius="6">
    <Entry.Behaviors>
        <behaviors:TextValidationBehavior
            InvalidColor="{StaticResource ErrorColor}"
            ValidColor="{StaticResource SuccessColor}"
            RegexPattern="^[a-zA-Z ]{2,100}$" />
    </Entry.Behaviors>
</Entry>
```

### Binding with String Formatting

```xaml
<!-- Formatting examples -->
<Label Text="{Binding Score, StringFormat='Score: {0:0.0}/100'}" />
<Label Text="{Binding CompletedDate, StringFormat='Completed on {0:MMM dd, yyyy}'}" />
<Label Text="{Binding StudyMinutes, StringFormat='{0} minutes'}" />
```

### Multi-Value Binding

```xaml
<!-- Display percentage and count together -->
<Grid ColumnDefinitions="*,Auto">
    <ProgressBar Progress="{Binding ProgressPercentage, Converter={StaticResource PercentageConverter}}" />
    <Label Grid.Column="1"
           TextColor="{StaticResource MediumGrayColor}">
        <Label.Text>
            <MultiBinding StringFormat="{0:0}% ({1}/{2})">
                <Binding Path="ProgressPercentage" />
                <Binding Path="CompletedLessons" />
                <Binding Path="TotalLessons" />
            </MultiBinding>
        </Label.Text>
    </Label>
</Grid>
```

### Binding with Value Converters

```csharp
// Converters/PercentageConverter.cs
using System.Globalization;

namespace SharpReady.Converters;

public class PercentageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            return percentage / 100.0;  // ProgressBar expects 0-1
        }
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            return progress * 100.0;
        }
        return 0.0;
    }
}

// Converters/BoolToColorConverter.cs
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var param = parameter as string;
        var colors = param?.Split(',') ?? new[] { "#007AFF", "#CCCCCC" };

        if (value is bool boolValue)
        {
            return Color.FromArgb(boolValue ? colors[0] : colors[1]);
        }
        return Color.FromArgb(colors[1]);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// Register in App.xaml
<ResourceDictionary>
    <converters:PercentageConverter x:Key="PercentageConverter" />
    <converters:BoolToColorConverter x:Key="BoolToColorConverter" />
</ResourceDictionary>
```

---

## Custom Controls

### Progress Ring Control

**File**: `Controls/ProgressRingControl.xaml`

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SharpReady.Controls.ProgressRingControl"
             Padding="0">

    <Grid>
        <!-- Background Circle -->
        <Ellipse Stroke="{Binding BackgroundColor, Source={RelativeSource Mode=Self}}"
                 StrokeThickness="{Binding LineWidth, Source={RelativeSource Mode=Self}}"
                 Fill="Transparent" />

        <!-- Progress Circle (drawn in code-behind) -->
        <Canvas x:Name="ProgressCanvas" />

        <!-- Center Label -->
        <Label Text="{Binding ProgressText}"
               FontSize="32"
               FontAttributes="Bold"
               TextColor="{Binding ProgressColor, Source={RelativeSource Mode=Self}}"
               HorizontalOptions="Center"
               VerticalOptions="Center" />
    </Grid>

</ContentView>
```

**File**: `Controls/ProgressRingControl.xaml.cs`

```csharp
namespace SharpReady.Controls;

public partial class ProgressRingControl : ContentView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(
            nameof(Progress),
            typeof(double),
            typeof(ProgressRingControl),
            0.0,
            propertyChanged: OnProgressChanged);

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.Create(
            nameof(Radius),
            typeof(double),
            typeof(ProgressRingControl),
            100.0);

    public static readonly BindableProperty LineWidthProperty =
        BindableProperty.Create(
            nameof(LineWidth),
            typeof(double),
            typeof(ProgressRingControl),
            8.0);

    public static readonly BindableProperty ProgressColorProperty =
        BindableProperty.Create(
            nameof(ProgressColor),
            typeof(Color),
            typeof(ProgressRingControl),
            Colors.Blue);

    public static readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(ProgressRingControl),
            Colors.LightGray);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public double LineWidth
    {
        get => (double)GetValue(LineWidthProperty);
        set => SetValue(LineWidthProperty, value);
    }

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public string ProgressText => $"{(int)(Progress * 100)}%";

    public ProgressRingControl()
    {
        InitializeComponent();
    }

    private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ProgressRingControl control)
        {
            control.OnProgressChanged();
        }
    }

    private void OnProgressChanged()
    {
        ProgressCanvas.Clear();
        DrawProgressRing();
    }

    private void DrawProgressRing()
    {
        // Implementation to draw the progress ring
        // (Uses SkiaSharp or canvas drawing APIs)
    }
}
```

---

### Expandable Section Control

**File**: `Controls/ExpandableSection.xaml`

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SharpReady.Controls.ExpandableSection">

    <VerticalStackLayout Spacing="0">
        <!-- Header (Tappable) -->
        <Grid ColumnDefinitions="*,Auto"
              Padding="16,12"
              BackgroundColor="{StaticResource LightGrayColor}"
              RowDefinitions="Auto">

            <TapGestureRecognizer Command="{Binding ToggleExpandedCommand, Source={RelativeSource AncestorType={x:Type ContentView}}}" />

            <Label Text="{Binding Title, Source={RelativeSource AncestorType={x:Type ContentView}}}"
                   FontSize="16"
                   FontAttributes="Bold"
                   TextColor="{StaticResource DarkGrayColor}"
                   VerticalOptions="Center" />

            <Label Grid.Column="1"
                   Text="{Binding IsExpanded, Source={RelativeSource AncestorType={x:Type ContentView}}, StringFormat='{0}'}"
                   FontSize="18"
                   TextColor="{StaticResource MediumGrayColor}"
                   VerticalOptions="Center">
                <Label.Text>
                    <MultiBinding StringFormat="{0}">
                        <Binding Path="IsExpanded" />
                    </MultiBinding>
                </Label.Text>
            </Label>
        </Grid>

        <!-- Content (Collapsible) -->
        <Frame IsVisible="{Binding IsExpanded, Source={RelativeSource AncestorType={x:Type ContentView}}}"
               BorderColor="Transparent"
               CornerRadius="0"
               Padding="16"
               Margin="0">
            <ContentPresenter />
        </Frame>
    </VerticalStackLayout>

</ContentView>
```

---

## Navigation Implementation

### Enhanced AppShell with Tab Navigation

**File**: `AppShell.xaml`

```xaml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="SharpReady.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:local="clr-namespace:SharpReady"
    xmlns:views="clr-namespace:SharpReady.Views"
    Title="SharpReady"
    BackgroundColor="{StaticResource WhiteColor}"
    ForegroundColor="{StaticResource DarkGrayColor}">

    <TabBar>
        <!-- Tab 1: Home -->
        <ShellContent
            Title="Home"
            Icon="home.png"
            ContentTemplate="{DataTemplate views:HomePage}"
            Route="Home" />

        <!-- Tab 2: Study Materials -->
        <ShellContent
            Title="Study"
            Icon="book.png"
            ContentTemplate="{DataTemplate views:StudyMaterialsPage}"
            Route="Study" />

        <!-- Tab 3: Progress -->
        <ShellContent
            Title="Progress"
            Icon="chart.png"
            ContentTemplate="{DataTemplate views:LearningProgressPage}"
            Route="Progress" />

        <!-- Tab 4: Profile -->
        <ShellContent
            Title="Profile"
            Icon="user.png"
            ContentTemplate="{DataTemplate views:ProfilePage}"
            Route="Profile" />
    </TabBar>

    <!-- Shell Routes (for nested navigation) -->
    <ShellContent Route="TopicDetails"
                  ContentTemplate="{DataTemplate views:TopicDetailsPage}" />

    <ShellContent Route="LessonViewer"
                  ContentTemplate="{DataTemplate views:LessonViewerPage}" />

    <ShellContent Route="Quiz"
                  ContentTemplate="{DataTemplate views:QuizPage}" />

    <ShellContent Route="Results"
                  ContentTemplate="{DataTemplate views:ResultsPage}" />

</Shell>
```

### Navigation Service Implementation

**File**: `Services/NavigationService.cs`

```csharp
using SharpReady.Services;

namespace DotNetStudyStudyAssistant.Services;

public class NavigationService : INavigationService
{
    public async Task GoToTopicDetailsAsync(int topicId)
    {
        await Shell.Current.GoToAsync($"TopicDetails?id={topicId}");
    }

    public async Task GoToLessonAsync(int topicId, int lessonId)
    {
        await Shell.Current.GoToAsync($"LessonViewer?topicId={topicId}&lessonId={lessonId}");
    }

    public async Task GoToQuizAsync(int topicId)
    {
        await Shell.Current.GoToAsync($"Quiz?topicId={topicId}");
    }

    public async Task GoToResultsAsync(int quizId)
    {
        await Shell.Current.GoToAsync($"Results?quizId={quizId}");
    }

    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task GoToHomeAsync()
    {
        await Shell.Current.GoToAsync("//Home");
    }
}
```

### Query Parameter Handling in ViewModels

```csharp
[QueryProperty(nameof(TopicId), "id")]
public partial class TopicDetailsPage : ContentPage
{
    private int topicId;

    public int TopicId
    {
        get => topicId;
        set
        {
            topicId = value;
            OnTopicIdChanged();
        }
    }

    private void OnTopicIdChanged()
    {
        if (BindingContext is TopicDetailsViewModel viewModel)
        {
            viewModel.LoadTopicCommand.Execute(TopicId);
        }
    }
}
```

---

## Animation & Effects

### Page Transition Animation

```csharp
public partial class TopicDetailsPage : ContentPage
{
    public TopicDetailsPage(TopicDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Fade in animation
        await MainContent.FadeTo(1, 300, Easing.CubicOut);
    }
}
```

### Button Press Animation

```xaml
<Button Text="Submit Answer"
        Command="{Binding SubmitAnswerCommand}"
        Pressed="OnButtonPressed"
        Released="OnButtonReleased" />
```

```csharp
private async void OnButtonPressed(object sender, EventArgs e)
{
    if (sender is Button button)
    {
        await button.ScaleTo(0.95, 100, Easing.CubicInOut);
    }
}

private async void OnButtonReleased(object sender, EventArgs e)
{
    if (sender is Button button)
    {
        await button.ScaleTo(1, 100, Easing.CubicInOut);
    }
}
```

### Loading Spinner with Auto-Dismiss Toast

```xaml
<ActivityIndicator IsRunning="{Binding IsLoading}"
                   IsVisible="{Binding IsLoading}"
                   Color="{StaticResource PrimaryColor}"
                   Scale="2" />
```

```csharp
private async Task ShowLoadingAsync(string message, int durationMs = 3000)
{
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        IsLoading = true;
        await Task.Delay(durationMs);
        IsLoading = false;
    });
}
```

---

## Responsive Design Code

### Helper for Adaptive Layouts

**File**: `Utilities/ResponsiveHelper.cs`

```csharp
namespace SharpReady.Utilities;

public static class ResponsiveHelper
{
    private static readonly double ScreenWidth = DeviceDisplay.MainDisplayInfo.Width;
    private static readonly double ScreenHeight = DeviceDisplay.MainDisplayInfo.Height;
    private static readonly double Density = DeviceDisplay.MainDisplayInfo.Density;

    public static int GetGridColumns(DeviceOrientation orientation = DeviceOrientation.Portrait)
    {
        var width = orientation == DeviceOrientation.Portrait ? ScreenWidth : ScreenHeight;
        return width / Density > 768 ? 3 : (width / Density > 480 ? 2 : 1);
    }

    public static double GetScaledFontSize(double baseSize)
    {
        return baseSize * (ScreenWidth / 360 / Density);
    }

    public static Thickness GetAdaptivePadding(double phone = 16, double tablet = 24)
    {
        var isTablet = ScreenWidth / Density > 768;
        var padding = isTablet ? tablet : phone;
        return new Thickness(padding);
    }

    public static GridItemsLayout GetAdaptiveGridLayout()
    {
        var columns = GetGridColumns();
        return new GridItemsLayout(columns)
        {
            HorizontalItemSpacing = 12,
            VerticalItemSpacing = 12
        };
    }
}
```

### Using Responsive Helper in XAML

```xaml
<CollectionView ItemsSource="{Binding Achievements}"
                ItemsLayout="{Binding AdaptiveGridLayout}">
</CollectionView>
```

```csharp
public class ProfileViewModel : BaseViewModel
{
    public GridItemsLayout AdaptiveGridLayout
        => ResponsiveHelper.GetAdaptiveGridLayout();
}
```

---

## State Management Patterns

### ViewModel with State Preservation

```csharp
public partial class StudyMaterialsViewModel : BaseViewModel
{
    private string searchQuery = string.Empty;
    private Category selectedCategory;
    private ObservableCollection<StudyTopic> filteredTopics;

    public string SearchQuery
    {
        get => searchQuery;
        set => SetProperty(ref searchQuery, value, OnSearchChanged);
    }

    public Category SelectedCategory
    {
        get => selectedCategory;
        set => SetProperty(ref selectedCategory, value, OnCategoryChanged);
    }

    public ObservableCollection<StudyTopic> FilteredTopics
    {
        get => filteredTopics;
        set => SetProperty(ref filteredTopics, value);
    }

    private void OnSearchChanged()
    {
        ApplyFilters();
    }

    private void OnCategoryChanged()
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var results = Topics
            .Where(t =>
            {
                bool matchesSearch = string.IsNullOrEmpty(SearchQuery) ||
                    t.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);

                bool matchesCategory = SelectedCategory == null ||
                    t.Category == SelectedCategory.Name;

                return matchesSearch && matchesCategory;
            })
            .ToList();

        FilteredTopics = new ObservableCollection<StudyTopic>(results);
    }
}
```

### Command Implementations with Async Support

```csharp
public partial class QuizViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    public ICommand SubmitAnswerCommand { get; }
    public ICommand NextQuestionCommand { get; }

    public QuizViewModel(IDataService dataService)
    {
        _dataService = dataService;

        SubmitAnswerCommand = new Command(
            execute: async () => await SubmitAnswerAsync(),
            canExecute: () => SelectedAnswer != null);

        NextQuestionCommand = new Command(
            execute: async () => await NextQuestionAsync(),
            canExecute: () => CurrentQuestionIndex < TotalQuestions - 1);
    }

    private async Task SubmitAnswerAsync()
    {
        try
        {
            IsBusy = true;

            // Check if answer is correct
            IsCorrect = SelectedAnswer.IsCorrect;
            if (IsCorrect)
            {
                CorrectAnswers++;
            }

            ShowFeedback = true;

            // Simulate processing delay
            await Task.Delay(2000);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NextQuestionAsync()
    {
        if (CurrentQuestionIndex < TotalQuestions - 1)
        {
            CurrentQuestionIndex++;
            CurrentQuestion = Questions[CurrentQuestionIndex];
            SelectedAnswer = null;
            ShowFeedback = false;
        }
    }
}
```

---

## Styling Best Practices

### Centralized Styles in App.xaml

**File**: `App.xaml`

```xaml
<?xml version = "1.0" encoding = "UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SharpReady.App">

    <Application.Resources>
        <ResourceDictionary>
            <!-- Import color resources -->
            <ResourceDictionary Source="Resources/Styles/ColorResources.xaml" />

            <!-- Import text styles -->
            <ResourceDictionary Source="Resources/Styles/TextStyles.xaml" />

            <!-- Import button styles -->
            <ResourceDictionary Source="Resources/Styles/ButtonStyles.xaml" />

            <!-- Default Styles -->
            <Style TargetType="Page"
                   ApplyToDerivedTypes="True">
                <Setter Property="BackgroundColor"
                        Value="{StaticResource WhiteColor}" />
            </Style>

            <Style TargetType="Label"
                   ApplyToDerivedTypes="True">
                <Setter Property="TextColor"
                        Value="{StaticResource DarkGrayColor}" />
                <Setter Property="FontSize" Value="14" />
            </Style>

            <Style TargetType="Button"
                   ApplyToDerivedTypes="True">
                <Setter Property="Padding" Value="16" />
                <Setter Property="FontSize" Value="16" />
                <Setter Property="CornerRadius" Value="8" />
            </Style>
        </ResourceDictionary>
    </Application.Resources>

</Application>
```

---

## Testing UI Components

### Unit Testing a ViewModel

```csharp
using Xunit;
using SharpReady.ViewModels;
using SharpReady.Services;
using Moq;

namespace SharpReady.Tests;

public class QuizViewModelTests
{
    [Fact]
    public async Task SubmitAnswerCommand_WithCorrectAnswer_IncrementsScore()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var viewModel = new QuizViewModel(mockDataService.Object);

        var correctAnswer = new Answer { Id = 1, IsCorrect = true };
        viewModel.SelectedAnswer = correctAnswer;
        var initialScore = viewModel.CorrectAnswers;

        // Act
        await viewModel.SubmitAnswerAsync();

        // Assert
        Assert.Equal(initialScore + 1, viewModel.CorrectAnswers);
        Assert.True(viewModel.IsCorrect);
    }

    [Fact]
    public void NextQuestionCommand_CanExecute_WhenNotOnLastQuestion()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var viewModel = new QuizViewModel(mockDataService.Object);
        viewModel.CurrentQuestionIndex = 0;
        viewModel.TotalQuestions = 6;

        // Act
        var canExecute = viewModel.NextQuestionCommand.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }
}
```

---

## Summary

This document provides practical, production-ready patterns for implementing the SharpReady UI. Key takeaways:

1. **XAML Patterns**: Use the sticky bottom bar pattern for consistent layouts
2. **Data Binding**: Leverage converters and multi-value bindings for flexibility
3. **Custom Controls**: Encapsulate complex UI logic in reusable controls
4. **Navigation**: Use Shell-based routing with query parameters
5. **Animations**: Keep animations subtle and performant
6. **Responsive Design**: Create adaptive layouts using helpers
7. **State Management**: Use ViewModels with proper property change notification
8. **Testing**: Write unit tests for ViewModels to ensure behavior

For more information, see:
- **UI_UX_PLAN.md**: Complete design specifications
- **SCREEN_WIREFRAMES.md**: Detailed wireframes for each screen
- **ARCHITECTURE.md**: Overall application architecture
