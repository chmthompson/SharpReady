# SharpReady - Front-End UI/UX Plan

**Date Created**: March 5, 2026
**Framework**: .NET MAUI
**Target Platform**: Android
**Design Pattern**: MVVM Architecture

---

## Table of Contents

1. [Design Philosophy](#design-philosophy)
2. [Navigation Architecture](#navigation-architecture)
3. [Screen Specifications](#screen-specifications)
4. [Component Library](#component-library)
5. [Layout Patterns](#layout-patterns)
6. [Visual Design System](#visual-design-system)
7. [Interaction Flows](#interaction-flows)
8. [Responsive Design](#responsive-design)
9. [Accessibility](#accessibility)
10. [Implementation Roadmap](#implementation-roadmap)

---

## Design Philosophy

### Core Principles

1. **Clarity**: Information hierarchy is clear and unambiguous
2. **Consistency**: Repeated patterns and components across all screens
3. **Responsiveness**: All interactions feel immediate and natural
4. **Accessibility**: Readable text, sufficient contrast, keyboard navigation
5. **Efficiency**: Minimal taps to complete tasks, streamlined workflows
6. **Material Design 3**: Following modern Android design standards

### Target Users

- .NET developers studying Android development
- Mobile app developers learning MAUI
- Students practicing C# and XAML

### UX Goals

- Onboard new users quickly with clear, focused interface
- Provide deep functionality without overwhelming complexity
- Enable power users to navigate efficiently
- Maintain visual consistency throughout the app
- Support offline functionality gracefully

---

## Navigation Architecture

### Navigation Model: Shell-Based Tab Navigation

**Current Structure:**
- TabBar with two primary tabs
  - Home (MainPage)
  - Profile (ProfilePage)

### Proposed Enhanced Navigation Structure

```
App Shell
├── Tab 1: Home
│   ├── Home Page (Dashboard)
│   ├── Study Plans (nested)
│   │   ├── Create Plan
│   │   ├── Plan Details
│   │   └── Study Sessions
│   └── Quick Actions (modal)
│
├── Tab 2: Study Materials
│   ├── Topics List
│   ├── Topic Details
│   │   ├── Content Viewer
│   │   ├── Quiz/Exercises
│   │   └── Resources
│   └── Search
│
├── Tab 3: My Learning
│   ├── Progress Dashboard
│   ├── Study History
│   ├── Certificates/Achievements
│   └── Performance Analytics
│
├── Tab 4: Profile
│   ├── Profile Overview
│   ├── Edit Profile
│   ├── Settings
│   │   ├── Notifications
│   │   ├── Preferences
│   │   └── Account
│   ├── Study Preferences
│   └── Logout
│
└── Global Navigation
    ├── Search (overlay)
    ├── Notifications (toast/badge)
    └── Error Handling (dialogs)
```

### Navigation Best Practices

- **Deep Linking**: Support direct navigation to specific study topics
- **Back Navigation**: Proper back stack management for Android
- **Modal vs. Pushed**:
  - Modals for quick actions (add item, delete confirmation)
  - Push navigation for sequential flows (study session → content → quiz)
- **Navigation Parameters**: Pass data safely between pages via route parameters
- **State Preservation**: Remember user position and scroll in lists

---

## Screen Specifications

### 1. Home/Dashboard Page

**Purpose**: Central hub showing user's learning status and quick actions

**Layout Structure**:
```
┌─────────────────────────────┐
│  Header: Greeting + Date    │  (Fixed)
├─────────────────────────────┤
│  [User Avatar] [Name]       │  (Profile Card)
├─────────────────────────────┤
│  Today's Goal Progress:     │  (Progress Ring)
│  ████████░░ 80%             │
├─────────────────────────────┤
│  Quick Stats (3-column grid)│
│  ┌──────┐ ┌──────┐ ┌──────┐│
│  │ 12   │ │ 847  │ │ 5    ││
│  │ Days │ │ Pts  │ │ New  ││
│  └──────┘ └──────┘ └──────┘│
├─────────────────────────────┤
│  Recent Activity (Scrollable)│
│  • Completed: Variables     │
│  • Earned: Advanced Badge   │
│  • Studied: 2h 15m today    │
├─────────────────────────────┤
│  [Continue Study] [New Plan]│  (Action Buttons)
└─────────────────────────────┘
```

**Components**:
- Greeting header with date
- User avatar circle (60px)
- Daily progress ring chart
- Quick stats cards (3x metric cards in grid)
- Activity feed with icons
- Primary action buttons
- Pull-to-refresh functionality

**Data Binding**:
```csharp
public string GreetingMessage { get; set; }  // "Good morning, Alex!"
public UserProfile CurrentUser { get; set; }
public DailyProgress TodayProgress { get; set; }
public List<QuickStat> QuickStats { get; set; }
public ObservableCollection<ActivityItem> RecentActivity { get; set; }
public ICommand ContinueStudyCommand { get; set; }
public ICommand CreateStudyPlanCommand { get; set; }
```

---

### 2. Study Materials/Topics List Page

**Purpose**: Browse and explore all available study topics

**Layout Structure**:
```
┌─────────────────────────────┐
│  [🔍 Search...] [Filter ⋮]  │  (Search Bar)
├─────────────────────────────┤
│  Category Pills (Horizontal) │
│  [All] [C#] [XAML] [Async]   │
├─────────────────────────────┤
│  List of Topics (Vertical)  │
│  ┌─────────────────────────┐│
│  │ ✓ Variables             ││
│  │ Difficulty: ⭐⭐        ││
│  │ Duration: 45 min        ││
│  │ Progress: ████░░░░░ 40% ││
│  └─────────────────────────┘│
│  ┌─────────────────────────┐│
│  │ ◌ Methods & Functions   ││
│  │ Difficulty: ⭐⭐⭐      ││
│  │ Duration: 90 min        ││
│  │ Progress: ░░░░░░░░░░ 0% ││
│  └─────────────────────────┘│
│  ... more topics             │
└─────────────────────────────┘
```

**Components**:
- Search bar with clear button
- Filter button (opens dropdown/modal)
- Horizontal scrollable category pills
- Topic list items with:
  - Completion indicator (✓/◌)
  - Title
  - Difficulty stars (1-5)
  - Duration estimate
  - Progress bar
  - Right-side menu (more options)

**Data Binding**:
```csharp
public string SearchQuery { get; set; }
public ObservableCollection<StudyTopic> Topics { get; set; }
public ObservableCollection<Category> Categories { get; set; }
public Category SelectedCategory { get; set; }
public ICommand SearchCommand { get; set; }
public ICommand FilterCommand { get; set; }
public ICommand SelectTopicCommand { get; set; }
```

---

### 3. Topic Details Page

**Purpose**: Display detailed content and learning materials for a specific topic

**Layout Structure**:
```
┌─────────────────────────────┐
│  < Back  │ Topic Title  │ ⋮ │  (Header)
├─────────────────────────────┤
│  Topic Banner Image         │
├─────────────────────────────┤
│  Metadata (Scrollable)      │
│  ⏱ Duration: 45 min         │
│  ⭐ Difficulty: Beginner    │
│  👥 Learners: 1,247         │
├─────────────────────────────┤
│  Tabs: [Overview] [Learn]   │
│  [Exercises] [Resources]    │
├─────────────────────────────┤
│  Content Viewer (Tab Content)│
│  • Lesson 1: Basics         │
│  • Lesson 2: Advanced       │
│  • Lesson 3: Best Practices │
├─────────────────────────────┤
│  [Start Lesson] [Resume]    │
│  [Take Quiz]                │
└─────────────────────────────┘
```

**Components**:
- Header with back button and options menu
- Hero image/banner
- Metadata cards
- Tab navigation (scrollable if many tabs)
- Content area (changes based on selected tab)
- Action buttons at bottom (sticky)

**Data Binding**:
```csharp
public StudyTopic CurrentTopic { get; set; }
public string TopicDescription { get; set; }
public List<Lesson> Lessons { get; set; }
public List<Exercise> Exercises { get; set; }
public int ProgressPercentage { get; set; }
public bool IsCompleted { get; set; }
public ICommand StartLessonCommand { get; set; }
public ICommand TakeQuizCommand { get; set; }
public ICommand ToggleFavoriteCommand { get; set; }
```

---

### 4. Study Session/Content Viewer Page

**Purpose**: Full-screen learning experience with focused content delivery

**Layout Structure**:
```
┌─────────────────────────────┐
│  < Back  │ Lesson 1/5 │ ⋮  │  (Header)
├─────────────────────────────┤
│  Progress: ████░░░░░░ 50%   │
├─────────────────────────────┤
│  Content Area (Scrollable)  │
│  ╔═════════════════════════╗│
│  ║ Lesson Title            ║│
│  ║                         ║│
│  ║ Content with text,      ║│
│  ║ images, code blocks,    ║│
│  ║ and interactive elements║│
│  ║                         ║│
│  ║ 📄 Code Example:        ║│
│  ║ var x = 10;             ║│
│  ║ Console.WriteLine(x);   ║│
│  ╚═════════════════════════╝│
├─────────────────────────────┤
│  [Mark as Read]             │
│  [< Previous] [Next >]      │
│  [Jump to Lesson]           │
└─────────────────────────────┘
```

**Components**:
- Immersive header with progress
- Scrollable content area
- Code blocks with syntax highlighting
- Inline images and diagrams
- Note-taking capability (future)
- Navigation buttons (previous/next)
- Bottom action bar (sticky)

**Data Binding**:
```csharp
public Lesson CurrentLesson { get; set; }
public int CurrentLessonIndex { get; set; }
public int TotalLessons { get; set; }
public string LessonContent { get; set; }  // Rich HTML/Markdown
public bool IsLessonMarked { get; set; }
public ICommand MarkAsCompleteCommand { get; set; }
public ICommand NextLessonCommand { get; set; }
public ICommand PreviousLessonCommand { get; set; }
```

---

### 5. Quiz/Assessment Page

**Purpose**: Interactive assessment with immediate feedback

**Layout Structure**:
```
┌─────────────────────────────┐
│  Quiz: Fundamentals  │ ⋮    │
├─────────────────────────────┤
│  Progress: ████░░░░░░ 50%   │
│  Score: 3/6 correct         │
├─────────────────────────────┤
│  Question Counter: 3 of 6   │
├─────────────────────────────┤
│  ┌─────────────────────────┐│
│  │ What is a variable?     ││
│  │                         ││
│  │ A) A container for data ││
│  │ B) A function name      ││
│  │ C) A type annotation    ││
│  │                         ││
│  │ * D) All of the above   ││  (* = selected)
│  └─────────────────────────┘│
├─────────────────────────────┤
│  [Submit Answer]            │
│  [Skip] [Hints available]   │
└─────────────────────────────┘

--- After Submission ---

│  ✓ Correct!                │
│                             │
│  Explanation:               │
│  Variables are containers   │
│  that hold values...        │
│                             │
│  [Next Question >]          │
```

**Components**:
- Quiz header with title
- Progress indicator
- Score display
- Question counter
- Question text
- Answer options (radio buttons/checkboxes)
- Submit button
- Feedback display (correct/incorrect)
- Explanation section
- Skip/Hint buttons

**Data Binding**:
```csharp
public Quiz CurrentQuiz { get; set; }
public Question CurrentQuestion { get; set; }
public int CurrentQuestionIndex { get; set; }
public int TotalQuestions { get; set; }
public int CorrectAnswers { get; set; }
public Answer SelectedAnswer { get; set; }
public bool ShowFeedback { get; set; }
public string FeedbackMessage { get; set; }
public ICommand SubmitAnswerCommand { get; set; }
public ICommand SkipQuestionCommand { get; set; }
public ICommand RequestHintCommand { get; set; }
```

---

### 6. Progress/Learning Analytics Page

**Purpose**: Visual representation of learning progress and performance

**Layout Structure**:
```
┌─────────────────────────────┐
│  My Learning Progress       │
├─────────────────────────────┤
│  Overview Section           │
│  Completed: 12 topics       │
│  In Progress: 5 topics      │
│  Total Points: 847          │
├─────────────────────────────┤
│  Weekly Activity Chart      │
│  (Bar chart showing days)   │
│   │                         │
│   │  ████ ████ ███░ ░░░░    │
│  0├─────────────────────────│
│   Mon Tue Wed Thu Fri       │
├─────────────────────────────┤
│  Topic Performance          │
│  Variables ████████░░ 80%   │
│  Methods ██████░░░░░░ 60%   │
│  Loops ████████████░░ 85%   │
│  Objects ████░░░░░░░ 40%    │
├─────────────────────────────┤
│  Achievements (Horizontal)  │
│  [🏆] [🎖️] [🥇] [🎯]       │
│  Streak 7d | Expert Mode    │
└─────────────────────────────┘
```

**Components**:
- Overview metrics cards
- Weekly activity bar chart
- Topic performance progress bars
- Achievements/badges grid
- Statistics summary
- Trend indicators

**Data Binding**:
```csharp
public int CompletedTopics { get; set; }
public int InProgressTopics { get; set; }
public int TotalPoints { get; set; }
public ObservableCollection<DailyActivity> WeeklyActivity { get; set; }
public ObservableCollection<TopicPerformance> TopicPerformances { get; set; }
public ObservableCollection<Achievement> Achievements { get; set; }
public int CurrentStreak { get; set; }
```

---

### 7. Profile Page

**Purpose**: User account management and preferences

**Layout Structure**:
```
┌─────────────────────────────┐
│  < Back  │ My Profile  │ ⋮  │
├─────────────────────────────┤
│  [Avatar (Large)]           │
│  Alex Johnson               │
│  alex@example.com           │
│  ⭐⭐⭐⭐⭐ (4.8 rating)      │
├─────────────────────────────┤
│  Account Section            │
│  ├─ Edit Profile ▶          │
│  ├─ Change Password ▶       │
│  └─ Email Preferences ▶     │
├─────────────────────────────┤
│  Learning Settings          │
│  ├─ Difficulty Level ▶      │
│  ├─ Daily Goal ▶            │
│  ├─ Notifications ▶         │
│  └─ Study Reminders ▶       │
├─────────────────────────────┤
│  Other                      │
│  ├─ About App ▶             │
│  ├─ Privacy Policy ▶        │
│  ├─ Help & Feedback ▶       │
│  └─ Logout                  │
└─────────────────────────────┘
```

**Components**:
- User avatar (large, 120px)
- User info section (name, email, rating)
- Settings list with disclosure indicators
- Section headers with dividers
- Logout button (destructive style)

**Data Binding**:
```csharp
public UserProfile CurrentUser { get; set; }
public double AverageRating { get; set; }
public StudySettings StudySettings { get; set; }
public NotificationPreferences NotificationPrefs { get; set; }
public ICommand EditProfileCommand { get; set; }
public ICommand LogoutCommand { get; set; }
```

---

## Component Library

### 1. Buttons

**Primary Button** (High Emphasis)
- Background: Brand color (#007AFF)
- Text: White, bold
- Padding: 16px vertical, 24px horizontal
- Corner radius: 8px
- Minimum width: 100%
- Usage: Main actions (Start, Save, Submit)

```xaml
<Button
    Text="Primary Action"
    BackgroundColor="#007AFF"
    TextColor="White"
    FontAttributes="Bold"
    Padding="24,16"
    CornerRadius="8"
    HorizontalOptions="Fill" />
```

**Secondary Button** (Medium Emphasis)
- Border: 2px solid brand color
- Text: Brand color
- Background: Transparent
- Usage: Alternative actions (Cancel, Skip)

```xaml
<Button
    Text="Secondary Action"
    BackgroundColor="Transparent"
    TextColor="#007AFF"
    BorderColor="#007AFF"
    BorderWidth="2"
    FontAttributes="Bold"
    Padding="24,16"
    CornerRadius="8"
    HorizontalOptions="Fill" />
```

**Tertiary Button** (Low Emphasis)
- Background: Light gray (#F2F2F7)
- Text: Dark gray (#333333)
- Usage: Less important actions (Learn More)

**Destructive Button**
- Background: Red (#FF3B30)
- Text: White
- Usage: Delete, Logout, Risky actions

---

### 2. Input Components

**Text Entry**
```xaml
<Entry
    Placeholder="Enter text..."
    PlaceholderColor="#999999"
    BackgroundColor="#F2F2F7"
    Padding="12"
    CornerRadius="6"
    FontSize="16" />
```

**Search Bar**
```xaml
<SearchBar
    Placeholder="Search..."
    PlaceholderColor="#999999"
    BackgroundColor="#F2F2F7"
    CancelButtonColor="#007AFF" />
```

**Radio Button Options**
```xaml
<RadioButton
    Content="Option A"
    Value="a"
    GroupName="answers"
    FontSize="16"
    Padding="12"
    CornerRadius="8"
    BorderColor="#E0E0E0"
    BorderWidth="1" />
```

---

### 3. List Components

**CollectionView with Uniform Grid**
- Used for: Achievement badges, quick stats
- Spacing: 12px
- Item size: Square cards

```xaml
<CollectionView ItemsSource="{Binding Achievements}">
    <CollectionView.ItemsLayout>
        <GridItemsLayout Orientation="Vertical"
                        HorizontalItemSpacing="12"
                        VerticalItemSpacing="12"
                        Rows="3" />
    </CollectionView.ItemsLayout>
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <!-- Achievement item -->
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

**CollectionView Linear (Vertical)**
- Used for: Topic lists, activity feed
- Item height: 80-120px
- Spacing: 8px between items

---

### 4. Indicators & Feedback

**Progress Bar**
- Height: 8px
- Corner radius: 4px
- Background: Light gray (#E0E0E0)
- Fill: Brand color (#007AFF) or green (#34C759) for success

**Activity Indicator (Loading Spinner)**
- Color: Brand color
- Size: 40x40px for full-screen, 24x24px for inline

**Toast Messages**
- Position: Bottom center with 16px margin
- Duration: 3 seconds
- Auto-dismiss

**Badges**
- Inline badges for "New", "Popular", "Featured"
- Small label styles

---

### 5. Cards & Containers

**Frame Card**
```xaml
<Frame
    BorderColor="#E0E0E0"
    CornerRadius="12"
    Padding="16"
    HasShadow="True"
    Margin="8">
    <!-- Content -->
</Frame>
```

**Expandable Section**
- Initially collapsed
- Shows title only
- Expands to show content
- Smooth animation transition

---

## Layout Patterns

### Pattern 1: List with Header

```
┌─────────────────────────┐
│ Header (Fixed)          │
├─────────────────────────┤
│ [Item 1]                │
├─────────────────────────┤
│ [Item 2]                │
├─────────────────────────┤
│ [Item 3]                │
│ ... (Scrollable)        │
└─────────────────────────┘
```

**Implementation**:
- Header in VerticalStackLayout with fixed height
- List in ScrollView below
- OR use CollectionView with header template

### Pattern 2: Bottom Action Bar (Sticky)

```
┌─────────────────────────┐
│ Scrollable Content      │
│ ...                     │
│ ... (ScrollView)        │
├─────────────────────────┤  (Separator)
│ [Action 1] [Action 2]   │  (Fixed at bottom)
└─────────────────────────┘
```

**Implementation**:
- Use Grid with 2 rows
- Row 1: * (fill remaining)
- Row 2: Auto (button bar height)

### Pattern 3: Expandable Sections

```
┌─────────────────────────┐
│ [▶] Section Title       │
├─────────────────────────┤
│ [▼] Section Title       │
│ ├─ Item 1               │
│ ├─ Item 2               │
│ └─ Item 3               │
├─────────────────────────┤
│ [▶] Section Title       │
└─────────────────────────┘
```

**Implementation**:
- TemplatedView or custom Control
- Toggle visibility of content on header tap
- Smooth height animation

### Pattern 4: Tab Navigation

```
┌─────────────────────────┐
│ ┌─────┬─────┬────────┐  │
│ │ Tab1│ Tab2│ Tab 3  │  │ (Horizontal Scroll)
│ └─────┴─────┴────────┘  │
├─────────────────────────┤
│ Content for Active Tab  │ (Swappable)
│ (changes on selection)  │
│                         │
│                         │
│                         │
└─────────────────────────┘
```

**Implementation**:
- TabView control (if available)
- OR custom implementation with:
  - Horizontal ScrollView for tabs
  - ContentView binding to selected tab index

---

## Visual Design System

### Color Palette

**Primary Colors**:
- Brand Blue: `#007AFF`
- Success Green: `#34C759`
- Warning Orange: `#FF9500`
- Error Red: `#FF3B30`
- Neutral Gray: `#8E8E93`

**Neutral Colors**:
- White: `#FFFFFF`
- Light Gray: `#F2F2F7`
- Medium Gray: `#D1D1D6`
- Dark Gray: `#333333`
- Black: `#000000`

**Semantic Colors**:
- Completed: Green (`#34C759`)
- In Progress: Blue (`#007AFF`)
- Not Started: Gray (`#D1D1D6`)
- Disabled: Light Gray (`#F2F2F7`)

### Typography

**Font Family**: System default (Roboto on Android)

**Type Scale**:
- Display: 32px, Bold
- Headline: 24px, Bold
- Title: 20px, SemiBold
- Body Large: 16px, Regular
- Body: 14px, Regular
- Label: 12px, Medium
- Caption: 11px, Regular

**Line Heights**:
- Display: 1.2x
- Headline: 1.3x
- Body: 1.5x
- Label: 1.4x

### Spacing Scale

Used consistently throughout the app:
- xs: 4px
- sm: 8px
- md: 12px
- lg: 16px
- xl: 24px
- 2xl: 32px
- 3xl: 48px

**Padding defaults**:
- Screen edges: 16px
- Card padding: 16px
- Element spacing: 8-12px

### Shadows

**Elevation Levels**:
- Elevated 1: 0 1px 3px rgba(0,0,0,0.12)
- Elevated 2: 0 3px 6px rgba(0,0,0,0.16)
- Elevated 3: 0 5px 12px rgba(0,0,0,0.19)

**Implementation**:
```xaml
<Frame HasShadow="True" .../>  <!-- Default elevation -->
```

### Corner Radius Standards

- Buttons: 8px
- Cards: 12px
- Input fields: 6px
- Large containers: 16px

---

## Interaction Flows

### Flow 1: Learning a Topic (Happy Path)

```
1. Dashboard
   ↓ [Continue Study] or [New Plan]
2. Topic Selection
   ↓ Select from list
3. Topic Details
   ↓ [Start Lesson]
4. Content Viewer (Lesson 1/5)
   ↓ [Next Lesson]
5. Content Viewer (Lesson 2/5)
   ↓ [Next Lesson]
   ...
6. Quiz
   ↓ Answer all questions
7. Results Screen
   ↓ [Review] or [Next Topic]
8. Dashboard (Updated)
```

**Data Flow**:
- ViewModel loads topic and lessons
- User progresses through lessons
- Each lesson completion updates:
  - `IsLessonMarked` property
  - User's `CompletedLessons` collection
  - Progress percentage
- Quiz answers sent to backend
- Results calculated and displayed

### Flow 2: Searching for a Topic

```
1. Study Materials Tab
   ↓ Tap Search Bar
2. Search Active (keyboard shown)
   ↓ Type "Variables"
3. Results Update (live as typing)
   ↓ Tap matching topic
4. Topic Details
```

**Implementation Notes**:
- SearchCommand executes on text changed
- Results filtered locally (or via API with debounce)
- Instant feedback as user types
- Clear button to reset search

### Flow 3: Quiz Taking with Feedback

```
1. Topic Details [Take Quiz]
   ↓
2. Quiz Start Screen
   ↓ [Begin]
   ↓
3. Question 1/6 (Display)
   ↓ Select Answer
   ↓ [Submit Answer]
   ↓
4. Feedback Display (Correct/Incorrect)
   ↓ [Show Explanation]
   ↓ [Next Question >]
   ↓
5. Question 2/6 ... (Loop)
   ↓
6. Final Results Screen
   ↓ Score: 5/6
   ↓ [Review Answers] [Retake Quiz]
```

**Quiz State Management**:
- Track current question index
- Store user answers in collection
- Calculate score in real-time
- Preserve answers for review

### Flow 4: Profile Editing

```
1. Profile Tab
   ↓ [Edit Profile]
   ↓
2. Edit Profile Page
   ├─ Name (Entry, Two-way binding)
   ├─ Email (Entry)
   ├─ Bio (Editor)
   └─ [Save Changes] [Cancel]
   ↓
3. Validation
   ├─ If valid → Save & navigate back
   └─ If invalid → Show error messages
```

---

## Responsive Design

### Breakpoints

**Phone (320-767px)**
- Single column layouts
- Full-width buttons
- Vertical stacking
- Bottom navigation/tabs

**Tablet (768px+)**
- Two-column layouts possible
- Larger spacing
- Side-by-side content
- Optimized for landscape

### Adaptive Patterns

**Grid Columns**:
```csharp
public static int GetGridColumns(double screenWidth) =>
    screenWidth > 768 ? 3 : (screenWidth > 480 ? 2 : 1);
```

**Font Scaling**:
```csharp
public static double GetScaledFontSize(double baseSize, double screenWidth) =>
    baseSize * (screenWidth / 360);  // Scale from 360px baseline
```

**List Item Height**:
- Phone: 80px (comfortable touch target)
- Tablet: 100px (more spacious)

---

## Accessibility

### Touch Targets

- Minimum size: 48x48dp (iOS) / 48x48px (Android)
- Spacing between targets: 8px minimum
- Buttons: Exactly 48-56px height

### Color Contrast

- Text on background: 4.5:1 contrast ratio minimum
- UI components: 3:1 contrast ratio minimum

**Color Combinations to Verify**:
- Blue text (#007AFF) on white background
- White text on blue background
- Gray labels (#666666) on light backgrounds

### Text

- Minimum font size: 12px (for captions)
- Body text: 14-16px
- Line height: 1.4x to 1.6x font size
- Avoid pure black on pure white (use #333 on #FFF)

### Labels & Descriptions

- All input fields labeled
- Images have descriptions (if meaningful)
- Buttons have clear text labels
- Icons accompanied by text

### Keyboard Navigation

- Tab order follows logical flow (top to bottom, left to right)
- Focus indicators visible
- Return key appropriate for context:
  - Search box: Execute search
  - Text input: Next field or Submit
  - Last field: Submit or Done

### Screen Reader Support

- Meaningful automation IDs on controls
- Proper semantic markup
- Announce loading states
- Describe complex layouts

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1-2)

**Objective**: Establish core UI structure and navigation

**Tasks**:
1. Enhance AppShell with 4-tab navigation
2. Create Dashboard/Home page with layout
3. Create StudyMaterials/Topics page with list
4. Establish color palette in App.xaml resources
5. Create reusable button styles
6. Set up base templates for card layouts

**Files to Create/Modify**:
- `AppShell.xaml` (enhanced)
- `Views/HomePage.xaml`
- `Views/StudyMaterialsPage.xaml`
- `Views/LearningProgressPage.xaml`
- `Views/SettingsPage.xaml`
- `Resources/Styles/Styles.xaml` (new)
- ViewModels for each page

---

### Phase 2: Content Pages (Week 3-4)

**Objective**: Implement topic details and learning flows

**Tasks**:
1. Create TopicDetailsPage with tab layout
2. Create LessonViewerPage (content viewer)
3. Create QuizPage with question display
4. Create ResultsPage
5. Implement navigation between pages
6. Add data binding for lesson content

**Files to Create/Modify**:
- `Views/TopicDetailsPage.xaml`
- `Views/LessonViewerPage.xaml`
- `Views/QuizPage.xaml`
- `Views/ResultsPage.xaml`
- ViewModels for each page
- Add models: `Lesson`, `Quiz`, `Question`, `Answer`

---

### Phase 3: Enhanced Features (Week 5-6)

**Objective**: Add interactive elements and visual polish

**Tasks**:
1. Implement search functionality with filtering
2. Add progress indicators and animations
3. Create custom controls (expandable sections, progress ring)
4. Implement offline-friendly states
5. Add loading and error states
6. Add achievements/badges display

**Files to Create/Modify**:
- `Controls/ProgressRingControl.xaml`
- `Controls/ExpandableSection.xaml`
- Service layer enhancements
- Add models: `Achievement`, `Badge`

---

### Phase 4: Polish & Optimization (Week 7-8)

**Objective**: Visual refinement and performance

**Tasks**:
1. Implement animations and transitions
2. Add pull-to-refresh functionality
3. Optimize image loading and caching
4. Test on multiple screen sizes
5. Implement accessibility features
6. Add local storage for offline support

**Files to Create/Modify**:
- Resource optimization
- Behavior implementations
- Converter implementations
- Platform-specific adjustments

---

## File Structure Reference

```
SharpReady/
├── Views/
│   ├── HomePage.xaml                    # Dashboard
│   ├── HomePage.xaml.cs
│   ├── StudyMaterialsPage.xaml          # Topics list
│   ├── StudyMaterialsPage.xaml.cs
│   ├── TopicDetailsPage.xaml            # Topic details with tabs
│   ├── TopicDetailsPage.xaml.cs
│   ├── LessonViewerPage.xaml            # Content viewer
│   ├── LessonViewerPage.xaml.cs
│   ├── QuizPage.xaml                    # Quiz/Assessment
│   ├── QuizPage.xaml.cs
│   ├── ResultsPage.xaml                 # Quiz results
│   ├── ResultsPage.xaml.cs
│   ├── LearningProgressPage.xaml        # Analytics
│   ├── LearningProgressPage.xaml.cs
│   ├── ProfilePage.xaml                 # User profile
│   ├── ProfilePage.xaml.cs
│   ├── SettingsPage.xaml                # Settings
│   └── SettingsPage.xaml.cs
│
├── ViewModels/
│   ├── HomePageViewModel.cs
│   ├── StudyMaterialsViewModel.cs
│   ├── TopicDetailsViewModel.cs
│   ├── LessonViewerViewModel.cs
│   ├── QuizViewModel.cs
│   ├── ResultsViewModel.cs
│   ├── LearningProgressViewModel.cs
│   ├── ProfileViewModel.cs
│   └── SettingsViewModel.cs
│
├── Models/
│   ├── BaseModel.cs
│   ├── UserProfile.cs
│   ├── StudyTopic.cs                    # New
│   ├── Lesson.cs                        # New
│   ├── Quiz.cs                          # New
│   ├── Question.cs                      # New
│   ├── Answer.cs                        # New
│   ├── Achievement.cs                   # New
│   ├── DailyProgress.cs                 # New
│   ├── QuickStat.cs                     # New
│   ├── TopicPerformance.cs              # New
│   └── ActivityItem.cs                  # New
│
├── Controls/
│   ├── ProgressRingControl.xaml         # New
│   ├── ProgressRingControl.xaml.cs
│   ├── ExpandableSection.xaml           # New
│   ├── ExpandableSection.xaml.cs
│   ├── StatCard.xaml                    # New
│   └── StatCard.xaml.cs
│
├── Resources/
│   ├── Styles/
│   │   ├── Styles.xaml                  # Enhanced
│   │   ├── ButtonStyles.xaml            # New
│   │   ├── TextStyles.xaml              # New
│   │   └── ColorResources.xaml          # New
│   ├── Converters/
│   │   ├── BoolToVisibilityConverter.cs
│   │   ├── IntToColorConverter.cs       # New
│   │   └── PercentageToAngleConverter.cs # New
│   ├── Images/
│   │   ├── icons/
│   │   ├── banners/
│   │   └── achievements/
│   └── Fonts/
│       └── (custom fonts if needed)
│
├── Services/
│   ├── IStudyService.cs                 # New
│   ├── StudyService.cs                  # New
│   ├── IQuizService.cs                  # New
│   ├── QuizService.cs                   # New
│   └── (existing services)
│
├── Utilities/
│   ├── Behaviors/
│   │   ├── NumericValidationBehavior.cs # New
│   │   └── EventToCommandBehavior.cs    # New
│   ├── Helpers/
│   │   ├── ResponsiveHelper.cs          # New
│   │   └── FormattingHelper.cs          # New
│   └── (existing utilities)
│
├── AppShell.xaml                        # Enhanced navigation
├── MauiProgram.cs                       # DI configuration
└── UI_UX_PLAN.md                        # This document
```

---

## Design System Implementation Guide

### Step 1: Create Color Resources

**File**: `Resources/Styles/ColorResources.xaml`

```xaml
<ResourceDictionary>
    <!-- Primary Colors -->
    <Color x:Key="PrimaryColor">#007AFF</Color>
    <Color x:Key="SuccessColor">#34C759</Color>
    <Color x:Key="WarningColor">#FF9500</Color>
    <Color x:Key="ErrorColor">#FF3B30</Color>

    <!-- Neutral Colors -->
    <Color x:Key="WhiteColor">#FFFFFF</Color>
    <Color x:Key="LightGrayColor">#F2F2F7</Color>
    <Color x:Key="MediumGrayColor">#D1D1D6</Color>
    <Color x:Key="DarkGrayColor">#333333</Color>
    <Color x:Key="BlackColor">#000000</Color>
</ResourceDictionary>
```

### Step 2: Create Text Styles

**File**: `Resources/Styles/TextStyles.xaml`

```xaml
<ResourceDictionary>
    <Style x:Key="HeadlineStyle" TargetType="Label">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="TextColor" Value="{StaticResource DarkGrayColor}" />
        <Setter Property="LineHeight" Value="1.3" />
    </Style>

    <Style x:Key="BodyStyle" TargetType="Label">
        <Setter Property="FontSize" Value="14" />
        <Setter Property="TextColor" Value="{StaticResource DarkGrayColor}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>
</ResourceDictionary>
```

### Step 3: Create Button Styles

**File**: `Resources/Styles/ButtonStyles.xaml`

```xaml
<ResourceDictionary>
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource PrimaryColor}" />
        <Setter Property="TextColor" Value="{StaticResource WhiteColor}" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="Padding" Value="24,16" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="FontSize" Value="16" />
    </Style>

    <Style x:Key="SecondaryButtonStyle" TargetType="Button">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource PrimaryColor}" />
        <Setter Property="BorderColor" Value="{StaticResource PrimaryColor}" />
        <Setter Property="BorderWidth" Value="2" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="Padding" Value="24,16" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="FontSize" Value="16" />
    </Style>
</ResourceDictionary>
```

---

## Notes & Future Considerations

### Dark Mode Support

Plan for future dark mode theme:
- Create parallel ColorResources_Dark.xaml
- Use OnPlatform for iOS/Android-specific colors
- Implement theme switching in settings

### Localization

Prepare for multi-language support:
- Extract all user-facing strings to RESX files
- Use `x:Static` bindings for resource access
- Support RTL layouts for Arabic/Hebrew

### Animation Framework

Consider MAUI Animation capabilities:
- Page transitions
- Button press animations
- List item entry animations
- Loading spinner animations

### State Management

As app grows, consider:
- MVVM Toolkit for advanced patterns
- Redux-like state management
- Event aggregation for cross-page communication

### Performance Optimizations

- Virtual scrolling for long lists
- Image lazy loading
- View recycling in lists
- Async data loading with cancellation tokens

---

## Conclusion

This UI/UX plan provides a comprehensive roadmap for building a modern, user-friendly Android application using MAUI. The design follows Material Design 3 principles and MVVM architectural patterns established in the project.

Key Takeaways:
1. **Navigation** is shell-based with clear tab structure
2. **Components** are reusable and consistent
3. **Layouts** follow established patterns
4. **Design System** ensures visual coherence
5. **Accessibility** is built-in from the start
6. **Roadmap** provides phased implementation approach

For questions or clarifications about specific design decisions, refer to the ARCHITECTURE.md file or consult Material Design documentation at https://m3.material.io
