# SharpReady - Screen Wireframes & Navigation Maps

This document provides detailed wireframes and user flow diagrams for all screens in the application.

---

## Application Navigation Map

```
┌─────────────────────────────────────────────────────────────────┐
│                      APP SHELL - TAB NAVIGATION                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Tab 1: Home          Tab 2: Study          Tab 3: Progress      │
│  (Dashboard)          (Materials)           (Analytics)          │
│  [🏠]                 [📚]                  [📊]                 │
│                                                                   │
│                                           Tab 4: Profile         │
│                                           (Settings)             │
│                                           [👤]                   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Screen 1: Home/Dashboard Page

### Wireframe

```
╔═══════════════════════════════════════╗
║ 🏠 Home                          ⋯   ║  Header (Fixed)
╠═══════════════════════════════════════╣
║                                       ║
║  👤 Alex Johnson                      ║  User Greeting Card
║  Good morning! ⭐⭐⭐⭐⭐ (23 ratings) ║
║                                       ║
║ ┌─────────────────────────────────┐  ║
║ │                                 │  ║  Daily Goal Progress Ring
║ │         87%                     │  ║
║ │    ████████░░                  │  ║
║ │   Goal: 120 min Study          │  ║
║ └─────────────────────────────────┘  ║
║                                       ║
║  Quick Stats (3-Column Grid)          ║
║  ┌──────────┐ ┌──────────┐ ┌───────┐ ║
║  │   12     │ │   847    │ │   5   │ ║
║  │ Days     │ │ Points   │ │ New   │ ║
║  │ Streak   │ │ Earned   │ │ Badges│ ║
║  └──────────┘ └──────────┘ └───────┘ ║
║                                       ║
║  Recent Activity                      ║
║  ├─ ✓ Completed: Variables Lesson     ║
║  ├─ 🏆 Earned: Consistent Learner    ║
║  ├─ 📝 Studied: 2h 15m today         ║
║  └─ ⭐ Started: Methods & Functions   ║
║                                       ║
║ ┌───────────────────┬───────────────┐ ║  Action Buttons (Sticky)
║ │ Continue Study    │ New Study Plan│ ║
║ └───────────────────┴───────────────┘ ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Data Model

```csharp
public class HomeViewModel : BaseViewModel
{
    public string GreetingMessage { get; set; }      // "Good morning, Alex!"
    public double DailyGoalProgress { get; set; }    // 0-100%
    public int DailyGoalMinutes { get; set; }        // Minutes studied today
    public int DailyGoalTarget { get; set; }         // Target minutes

    public int DayStreak { get; set; }               // 12
    public int TotalPoints { get; set; }             // 847
    public int NewBadgesCount { get; set; }          // 5

    public ObservableCollection<ActivityItem> RecentActivity { get; set; }

    public ICommand ContinueStudyCommand { get; set; }
    public ICommand CreateStudyPlanCommand { get; set; }
}

public class ActivityItem
{
    public string Icon { get; set; }                 // "✓", "🏆", "📝", etc.
    public string Message { get; set; }              // "Completed: Variables Lesson"
    public DateTime Timestamp { get; set; }
    public ActivityType Type { get; set; }           // Enum
}
```

### Key UX Decisions

- **Progress Ring**: Shows visual goal completion at a glance
- **Quick Stats**: Three metrics highlight user achievement
- **Activity Feed**: Recent actions provide context and motivation
- **Sticky Action Bar**: Primary actions always accessible without scrolling
- **Pull-to-Refresh**: Update data with familiar gesture

---

## Screen 2: Study Materials List

### Wireframe

```
╔═══════════════════════════════════════╗
║ 📚 Study Materials              ⋯   ║  Header
╠═══════════════════════════════════════╣
║                                       ║
║  [🔍 Search topics...]                ║  Search Bar
║                                       ║
║  [All] [C#] [XAML] [Async] [Types]   ║  Category Pills (Scrollable)
║                                       ║
║  Topic: Variables                     ║  Topic Item 1
║  ▪▪▪▪▪▪░░░░ 60% | ⭐⭐   | 45 min     ║
║                                       ║
║  Topic: Methods & Functions           ║  Topic Item 2
║  ░░░░░░░░░░  0% | ⭐⭐⭐ | 90 min     ║
║                                       ║
║  Topic: XAML Fundamentals             ║  Topic Item 3
║  ▪▪▪▪░░░░░░ 40% | ⭐⭐   | 75 min     ║
║                                       ║
║  Topic: Async/Await Programming       ║  Topic Item 4
║  ░░░░░░░░░░  0% | ⭐⭐⭐⭐| 120 min    ║
║                                       ║
║  Topic: Collections & LINQ            ║  Topic Item 5
║  ▪▪▪▪▪░░░░░ 50% | ⭐⭐⭐ | 100 min    ║
║                                       ║
║  ... (Scrollable)                     ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Data Model

```csharp
public class StudyMaterialsViewModel : BaseViewModel
{
    public string SearchQuery { get; set; }
    public ObservableCollection<StudyTopic> Topics { get; set; }
    public ObservableCollection<Category> Categories { get; set; }
    public Category SelectedCategory { get; set; }

    public ICommand SearchCommand { get; set; }
    public ICommand SelectTopicCommand { get; set; }
    public ICommand FilterCommand { get; set; }
}

public class StudyTopic : BaseModel
{
    public string Title { get; set; }                // "Variables"
    public string Description { get; set; }
    public int DifficultyLevel { get; set; }         // 1-5 stars
    public int EstimatedMinutes { get; set; }        // 45
    public double ProgressPercentage { get; set; }   // 0-100
    public bool IsCompleted { get; set; }
    public string Category { get; set; }             // "C#"
    public int LessonCount { get; set; }
    public int ExerciseCount { get; set; }
    public List<Lesson> Lessons { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }                 // "C#", "XAML"
    public int TopicCount { get; set; }
}
```

### Interaction Details

- **Search**: Live filtering as user types
- **Categories**: Horizontal scroll, shows count of topics
- **Topic Items**: Tap to navigate to TopicDetailsPage
- **Progress Indicator**: Visual bar with percentage
- **Difficulty Stars**: Help set expectations

---

## Screen 3: Topic Details Page (With Tabs)

### Wireframe

```
╔═══════════════════════════════════════╗
║ < Back  │ Variables              ⋯  ║  Header
╠═══════════════════════════════════════╣
║                                       ║
║  ┌─────────────────────────────────┐ ║
║  │                                 │ ║  Hero Image/Banner
║  │        [Variables Banner]       │ ║
║  │                                 │ ║
║  └─────────────────────────────────┘ ║
║                                       ║
║  Metadata Section                     ║
║  ⏱ Duration: 45 minutes               ║
║  ⭐ Difficulty: Beginner               ║
║  👥 Learners: 1,247                   ║
║  ✓ Your Progress: 60%                 ║
║                                       ║
║  ┌────────┬────────┬─────────┬──────┐║
║  │Overview│ Learn  │Exercise │Res.  ││  Tabs
║  └────────┴────────┴─────────┴──────┘║
║                                       ║
║  TAB CONTENT: Overview                ║
║  ┌─────────────────────────────────┐ ║
║  │ What is a Variable?             │ ║
║  │                                 │ ║
║  │ A variable is a named storage   │ ║
║  │ location in memory that holds   │ ║
║  │ a value. Variables allow us to  │ ║
║  │ store, manipulate, and reuse    │ ║
║  │ data in our programs.           │ ║
║  │                                 │ ║
║  │ Key Topics:                     │ ║
║  │ • Declaration and Initialization│ ║
║  │ • Data Types                    │ ║
║  │ • Scope and Lifetime            │ ║
║  │ • Best Practices                │ ║
║  └─────────────────────────────────┘ ║
║                                       ║
║ ┌─────────────────┬─────────────────┐║  Action Bar (Sticky)
║ │ Start Learning  │ Continue Study  ││
║ └─────────────────┴─────────────────┘║
║                                       ║
╚═══════════════════════════════════════╝
```

### Tab Content Variations

#### Tab 1: Overview
- Topic description
- Learning objectives
- Key topics list
- Prerequisites

#### Tab 2: Learn
- List of lessons
- Each lesson with:
  - Lesson number
  - Title
  - Duration
  - Completion status
  - "Start" or "Resume" button

#### Tab 3: Exercises
- Practice problems
- Difficulty level
- Points earned
- "Solve" button

#### Tab 4: Resources
- External links
- Reference materials
- Code samples
- Documentation

### Data Model

```csharp
public class TopicDetailsViewModel : BaseViewModel
{
    public StudyTopic Topic { get; set; }
    public string TopicDescription { get; set; }
    public List<string> LearningObjectives { get; set; }
    public List<string> KeyTopics { get; set; }

    public ObservableCollection<Lesson> Lessons { get; set; }
    public ObservableCollection<Exercise> Exercises { get; set; }
    public ObservableCollection<Resource> Resources { get; set; }

    public int SelectedTabIndex { get; set; }

    public ICommand StartLearningCommand { get; set; }
    public ICommand SelectLessonCommand { get; set; }
}
```

---

## Screen 4: Lesson Viewer (Content Page)

### Wireframe

```
╔═══════════════════════════════════════╗
║ < Back │ Lesson 1/5 │ ⋯             ║  Header
╠═══════════════════════════════════════╣
║ ▪▪▪▪▪░░░░░░░░░░░░░░░░ 20%  [1m 12s] ║  Progress Bar
├───────────────────────────────────────╢
║                                       ║
║  LESSON CONTENT (Scrollable)          ║
║                                       ║
║  Variables Fundamentals               ║
║                                       ║
║  What is a Variable?                  ║
║                                       ║
║  A variable is a named storage        ║
║  location in memory. Think of it      ║
║  like a labeled box where you can     ║
║  store data.                          ║
║                                       ║
║  Example:                             ║
║  ┌────────────────────────────────┐  ║
║  │ int age = 25;                  │  ║
║  │ string name = "Alice";         │  ║
║  │ bool isActive = true;          │  ║
║  └────────────────────────────────┘  ║
║                                       ║
║  Explanation:                         ║
║  • int: Integer data type             ║
║  • age: Variable name                 ║
║  • 25: Initial value                  ║
║                                       ║
║  ... (more content)                   ║
║                                       ║
╠═══════════════════════════════════════╣
║ [Mark Complete] [< Previous] [Next >]║  Action Bar (Sticky)
╚═══════════════════════════════════════╝
```

### Content Rendering

Content supports:
- **Headings** (H1, H2, H3)
- **Paragraphs** (formatted text)
- **Code Blocks** (syntax highlighting)
- **Lists** (ordered/unordered)
- **Images** (inline)
- **Quotes/Notes** (highlighted sections)
- **Tables** (scrollable if wide)

### Data Model

```csharp
public class LessonViewerViewModel : BaseViewModel
{
    public Lesson CurrentLesson { get; set; }
    public StudyTopic ParentTopic { get; set; }
    public int CurrentLessonIndex { get; set; }
    public int TotalLessons { get; set; }

    public string LessonContent { get; set; }        // HTML or Markdown
    public bool IsLessonMarked { get; set; }
    public TimeSpan ElapsedTime { get; set; }

    public bool CanGoToPrevious => CurrentLessonIndex > 0;
    public bool CanGoToNext => CurrentLessonIndex < TotalLessons - 1;

    public ICommand MarkCompleteCommand { get; set; }
    public ICommand NextLessonCommand { get; set; }
    public ICommand PreviousLessonCommand { get; set; }
}

public class Lesson : BaseModel
{
    public string Title { get; set; }
    public string Content { get; set; }              // HTML/Markdown
    public int DurationMinutes { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

---

## Screen 5: Quiz Page

### Wireframe - Question Display

```
╔═══════════════════════════════════════╗
║ Quiz: Variables Basics      [Help ⓘ] ║  Header
╠═══════════════════════════════════════╣
║                                       ║
║  Progress: ▪▪▪▪░░░░░░░░░░░░░░ 30%   ║
║  Score: 3/6 correct                   ║
║  Question: 3 of 6                     ║
║                                       ║
│ ┌─────────────────────────────────┐  ║
│ │ What is the purpose of a        │  ║  Question Card
│ │ variable?                       │  ║
│ │                                 │  ║
│ │ A) To store data temporarily    │  ║
│ │                                 │  ║
│ │ B) To declare a function        │  ║
│ │                                 │  ║
│ │ * C) Both A and B                │  ║ (* = selected)
│ │                                 │  ║
│ │ D) None of the above            │  ║
│ │                                 │  ║
│ └─────────────────────────────────┘  ║
║                                       ║
║ [Get Hint (3 left)]  [Skip Question] ║  Utility Buttons
║                                       ║
║ ┌──────────────────────────────────┐ ║
║ │ SUBMIT ANSWER                    │ ║  Submit Button (Prominent)
║ └──────────────────────────────────┘ ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Wireframe - Feedback Display

```
╔═══════════════════════════════════════╗
║ Quiz: Variables Basics      [Help ⓘ] ║
╠═══════════════════════════════════════╣
║                                       ║
║  Progress: ▪▪▪▪▪░░░░░░░░░░░░░ 50%   ║
║  Score: 4/6 correct                   ║
║  Question: 4 of 6                     ║
║                                       ║
║  ✓ CORRECT!                           ║  Feedback Badge
║                                       ║
║  Selected: "Both A and B"             ║
║                                       ║
║  Explanation:                         ║
║  Variables serve two primary          ║
║  purposes: they store data during     ║
║  program execution AND they give      ║
║  meaningful names to values in our    ║
║  code, making it more readable.       ║
║                                       ║
║  Key Points:                          ║
║  • Variables are memory locations     ║
║  • They have names and types          ║
║  • Values can change during execution ║
║                                       ║
║ ┌──────────────────────────────────┐ ║
║ │ NEXT QUESTION >                  │ ║
║ └──────────────────────────────────┘ ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Data Model

```csharp
public class QuizViewModel : BaseViewModel
{
    public Quiz CurrentQuiz { get; set; }
    public Question CurrentQuestion { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public Answer SelectedAnswer { get; set; }
    public bool ShowFeedback { get; set; }
    public string FeedbackMessage { get; set; }
    public bool IsCorrect { get; set; }

    public int HintsRemaining { get; set; }
    public string CurrentHint { get; set; }

    public double ScorePercentage => (CorrectAnswers * 100.0) / TotalQuestions;

    public ICommand SubmitAnswerCommand { get; set; }
    public ICommand SkipQuestionCommand { get; set; }
    public ICommand RequestHintCommand { get; set; }
    public ICommand NextQuestionCommand { get; set; }
}

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; }
    public List<Answer> Options { get; set; }
    public int CorrectAnswerId { get; set; }
    public string Explanation { get; set; }
    public string Hint { get; set; }
}

public class Answer
{
    public int Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}
```

---

## Screen 6: Quiz Results Page

### Wireframe

```
╔═══════════════════════════════════════╗
║ Quiz Results                    ⋯    ║
╠═══════════════════════════════════════╣
║                                       ║
║           QUIZ COMPLETE! ✓            ║
║                                       ║
║  Your Score:                          ║
║                                       ║
║     ███████░░                         ║  Score Ring
║        5 of 6                         ║
║       83% PASS ✓                      ║
║                                       ║
║  Performance:                         ║
║  ├─ Time Taken: 8 minutes 45 seconds ║
║  ├─ Accuracy: 83%                    ║
║  └─ Difficulty: ⭐⭐ Beginner         ║
║                                       ║
║  Points Earned:                       ║
║  + 150 pts (Quiz Completion)          ║
║  + 50 pts (Perfect Streak)            ║
║  ─────────────────────────────       ║
║  = 200 Total Points                   ║
║                                       ║
║  Achievements Unlocked:               ║
║  🏆 Completionist (5 topics done)     ║
║  🎯 Quiz Master (10 quizzes passed)   ║
║                                       ║
║  ┌─────────────────┬─────────────┐   ║
║  │ Review Answers  │ Next Topic >│   ║
║  └─────────────────┴─────────────┘   ║
║                                       ║
║  [Back to Study Materials]            ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Data Model

```csharp
public class ResultsViewModel : BaseViewModel
{
    public Quiz CompletedQuiz { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public double ScorePercentage { get; set; }
    public bool IsPassed { get; set; }

    public TimeSpan TimeTaken { get; set; }
    public int PointsEarned { get; set; }

    public List<AchievementUnlocked> NewAchievements { get; set; }
    public List<QuestionReview> QuestionReviews { get; set; }

    public ICommand ReviewAnswersCommand { get; set; }
    public ICommand NextTopicCommand { get; set; }
    public ICommand RetakeQuizCommand { get; set; }
}

public class AchievementUnlocked
{
    public string Badge { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}
```

---

## Screen 7: Learning Progress/Analytics

### Wireframe

```
╔═══════════════════════════════════════╗
║ 📊 My Learning Progress         ⋯   ║
╠═══════════════════════════════════════╣
║                                       ║
║  OVERVIEW METRICS                     ║
║  ┌──────────┬──────────┬──────────┐  ║
║  │    12    │    5     │   847    │  ║
║  │ Completed│In Progress│ Points  │  ║
║  │  Topics  │  Topics  │  Earned │  ║
║  └──────────┴──────────┴──────────┘  ║
║                                       ║
║  WEEKLY ACTIVITY                      ║
║    │                                  ║
║  2h│     ▓▓      ▓▓         ▓▓       ║  (Bar chart)
║  1h│     ▓▓      ▓▓    ▓▓   ▓▓ ▓▓    ║
║  0h├─────┴──────┴──┴──┴──┴─┴───┴────  ║
║    │ Mon Tue Wed Thu Fri Sat Sun     ║
║                                       ║
║  TOPIC PERFORMANCE                    ║
║  Variables                            ║
║  ████████████░░░░░░ 80% | Quiz: 5/6 ║
║                                       ║
║  Methods & Functions                  ║
║  ██████░░░░░░░░░░░░ 60% | Quiz: 4/8 ║
║                                       ║
║  XAML Fundamentals                    ║
║  ████████████████░░ 85% | Quiz: 7/8 ║
║                                       ║
║  Loops & Control Flow                 ║
║  ████░░░░░░░░░░░░░░ 40% | Quiz: 0/5 ║
║                                       ║
║  ACHIEVEMENTS (Horizontal)            ║
║  ┌──┐  ┌──┐  ┌──┐  ┌──┐  ┌──┐      ║
║  │🏆│  │🎖️│  │🥇│  │🎯│  │⭐│  ... ║
║  │  │  │  │  │  │  │  │  │  │      ║
║  └──┘  └──┘  └──┘  └──┘  └──┘      ║
║                                       ║
║  MILESTONES                           ║
║  ✓ 7 Day Streak - Keep it going!     ║
║  ⭕ 50 Points Away - Almost to 900!  ║
║  ⭕ 3 More Topics - To Level 5!      ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Data Model

```csharp
public class LearningProgressViewModel : BaseViewModel
{
    // Overview Stats
    public int CompletedTopics { get; set; }
    public int InProgressTopics { get; set; }
    public int TotalPoints { get; set; }

    // Weekly Activity
    public ObservableCollection<DailyActivity> WeeklyActivity { get; set; }

    // Topic Performance
    public ObservableCollection<TopicPerformance> TopicPerformances { get; set; }

    // Achievements
    public ObservableCollection<Achievement> Achievements { get; set; }

    // Milestones
    public ObservableCollection<Milestone> Milestones { get; set; }
    public int CurrentStreak { get; set; }
}

public class DailyActivity
{
    public DayOfWeek Day { get; set; }
    public int MinutesStudied { get; set; }
}

public class TopicPerformance
{
    public string TopicName { get; set; }
    public double CompletionPercentage { get; set; }
    public int QuizScore { get; set; }
    public int QuizTotal { get; set; }
}

public class Achievement
{
    public string Badge { get; set; }      // Emoji or icon
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime UnlockedDate { get; set; }
    public bool IsUnlocked { get; set; }
}
```

---

## Screen 8: Profile Page

### Wireframe

```
╔═══════════════════════════════════════╗
║ < Back │ My Profile           │ ⋯   ║
╠═══════════════════════════════════════╣
║                                       ║
║          👤                           ║
║       [Avatar]                        ║
║                                       ║
║    Alex Johnson                       ║
║    alex@example.com                   ║
║                                       ║
║    ⭐⭐⭐⭐⭐ 4.8/5.0                   ║
║    Excellent Learner                  ║
║                                       ║
├───────────────────────────────────────┤
║ ACCOUNT SETTINGS                      ║
║ ├─ Edit Profile                    ▶  ║
║ ├─ Change Password                 ▶  ║
║ ├─ Email Preferences               ▶  ║
║ └─ Privacy Settings                ▶  ║
├───────────────────────────────────────┤
║ LEARNING SETTINGS                     ║
║ ├─ Difficulty Level                ▶  ║
║ ├─ Daily Goal                      ▶  ║
║ ├─ Study Reminders                 ▶  ║
║ └─ Notification Preferences        ▶  ║
├───────────────────────────────────────┤
║ OTHER                                 ║
║ ├─ About SharpReady      ▶  ║
║ ├─ Privacy Policy                  ▶  ║
║ ├─ Terms & Conditions              ▶  ║
║ ├─ Help & Feedback                 ▶  ║
║ └─ Share App                       ▶  ║
├───────────────────────────────────────┤
║ ┌──────────────────────────────────┐  ║
║ │ SIGN OUT                         │  ║  Destructive Action
║ └──────────────────────────────────┘  ║
║                                       ║
║ App Version: 1.0.0                    ║
║                                       ║
╚═══════════════════════════════════════╝
```

### Data Model

```csharp
public class ProfileViewModel : BaseViewModel
{
    public UserProfile CurrentUser { get; set; }
    public double AverageRating { get; set; }
    public string RatingDescription { get; set; }    // "Excellent Learner"

    public StudySettings StudySettings { get; set; }
    public NotificationPreferences NotificationPrefs { get; set; }

    public ICommand EditProfileCommand { get; set; }
    public ICommand ChangePasswordCommand { get; set; }
    public ICommand SignOutCommand { get; set; }
}
```

---

## Navigation Flow Diagrams

### Learning Path Flow

```
Dashboard
   │
   ├─→ [Continue Study]
   │      │
   │      └─→ Topic Details
   │             │
   │             └─→ Lesson Viewer (Lesson 1)
   │                    │
   │                    ├─→ [Next] → Lesson 2
   │                    │               │
   │                    │               └─→ [Next] → ... → Lesson N
   │                    │                               │
   │                    │                               └─→ Quiz
   │                    │                                    │
   │                    │                                    └─→ Results
   │                    │
   │                    └─→ [Skip to Quiz]
   │
   └─→ [New Study Plan]
          │
          └─→ Study Materials (Topic Selection)
```

### Settings/Account Flow

```
Profile Tab
   │
   ├─→ [Edit Profile]
   │      └─→ Edit Form → [Save/Cancel]
   │
   ├─→ [Learning Settings]
   │      ├─→ Difficulty Level → [Change]
   │      ├─→ Daily Goal → [Set]
   │      └─→ Study Reminders → [Configure]
   │
   ├─→ [Notification Preferences]
   │      └─→ Toggle Switches
   │
   └─→ [Sign Out]
          └─→ Confirmation Dialog
```

---

## Dialog & Modal Designs

### Confirmation Dialog

```
┌──────────────────────────────┐
│  Confirm Action              │
├──────────────────────────────┤
│                              │
│  Are you sure you want to    │
│  delete this progress?       │
│                              │
│  This action cannot be       │
│  undone.                     │
│                              │
├──────────────────────────────┤
│ [Cancel]           [Delete]  │
│                              │
└──────────────────────────────┘
```

### Loading Dialog

```
┌──────────────────────────────┐
│                              │
│  Loading your data...        │
│                              │
│  ◌ (Spinner animation)       │
│                              │
│                              │
└──────────────────────────────┘
```

### Error Toast

```
Bottom of screen:
┌──────────────────────────────┐
│ ⚠ Error loading data         │
│ Please check your connection │
└──────────────────────────────┘
(Auto-dismiss after 3 seconds)
```

---

## Responsive Considerations

### Phone Layout (320-480px)
- Single column
- Full-width buttons
- Vertical stacking
- Bottom action bars
- Larger touch targets (48x48px minimum)

### Large Phone (481-767px)
- 2-column grid layouts possible
- Improved spacing
- Landscape support

### Tablet Layout (768px+)
- 3-4 column grids
- Sidebar navigation option
- Master-detail views
- Optimized spacing for larger screens

### Example: Adaptive Grid

```csharp
// QuickStats Grid
<CollectionView ItemsSource="{Binding QuickStats}">
    <CollectionView.ItemsLayout>
        <GridItemsLayout
            Orientation="Vertical"
            HorizontalItemSpacing="12"
            VerticalItemSpacing="12"
            Rows="{Binding GridRows}" />  // 1 on phone, 2+ on tablet
    </CollectionView.ItemsLayout>
</CollectionView>
```

---

## Animation & Micro-Interactions

### Page Transitions
- **Fade In**: When content loads
- **Slide Up**: When modals appear
- **Slide Down**: When closing modals
- **Scale**: Quiz result badges

### Button States
- **Normal**: Gray text on transparent background
- **Hover/Pressed**: Background color change, slight scale
- **Disabled**: Reduced opacity, disabled state

### Loading States
- **Content Loading**: Skeleton loaders for lists
- **Data Fetching**: Activity indicator with message
- **Auto-dismiss**: Toast messages fade out

### Progress Indicators
- **Animated**: Circular progress rings
- **Smooth**: Bar progress updates
- **Visual Feedback**: Color change on completion

---

## Accessibility Specifications

### Font Sizes
- Minimum body text: 14px
- Labels: 12px minimum
- Headlines: 20px+
- All text must be readable without zoom

### Color Contrast
- Normal text: 4.5:1 ratio
- Large text (18pt+): 3:1 ratio
- UI components: 3:1 ratio

### Touch Targets
- Minimum: 48x48dp
- Optimal spacing: 56-64dp
- Icons: 24x24dp with larger touch area

### Labels & Descriptions
- All buttons have descriptive text
- All icons have alt text
- Form fields have associated labels
- Important elements announced to screen readers

---

## Conclusion

This wireframe document provides the complete visual and interaction specification for the SharpReady application. Each screen is designed following MAUI best practices and Material Design 3 principles, with consideration for responsive design and accessibility.

For implementation details, refer to **UI_UX_PLAN.md** and **ARCHITECTURE.md**.
