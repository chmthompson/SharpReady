# SharpReady - UI/UX Quick Reference Guide

**Quick navigation guide for the UI/UX planning documents**

---

## Document Overview

| Document | Purpose | Size | Focus |
|----------|---------|------|-------|
| **UI_UX_PLAN.md** | Complete design specification | 41 KB | Design system, screens, patterns, roadmap |
| **SCREEN_WIREFRAMES.md** | Visual wireframes & flows | 38 KB | Screen layouts, wireframes, interactions |
| **UI_IMPLEMENTATION_PATTERNS.md** | Code examples & patterns | 39 KB | XAML, C#, navigation, animations |
| **UI_UX_SUMMARY.md** | Executive summary | 13 KB | Overview, quick facts, next steps |
| **UI_QUICK_REFERENCE.md** | This document | Quick ref | Navigation, checklists, key info |

---

## 30-Second Overview

**What**: Complete UI/UX design for SharpReady Android app
**Design Pattern**: MAUI Shell with 4-tab navigation
**Screens**: 8 fully specified screens (Dashboard, Topics, Lesson, Quiz, Results, Analytics, Profile, Settings)
**Colors**: Material Design 3 palette (Blue #007AFF, Green #34C759, Orange #FF9500, Red #FF3B30)
**Navigation**: Shell-based routing with query parameters
**Accessibility**: WCAG compliant with responsive design

---

## Which Document Should I Read?

### I want to understand the overall design
→ Read: **UI_UX_SUMMARY.md**
- 5 min read for executive overview
- See design decisions & key numbers
- Understand implementation roadmap

### I need to design a new screen
→ Read: **UI_UX_PLAN.md** (Sections 4-6)
- Component library specifications
- Layout patterns
- Design system (colors, typography, spacing)

### I'm implementing a specific screen
→ Read: **SCREEN_WIREFRAMES.md** (Specific screen section)
- Wireframe ASCII diagram
- Data model structure
- Key UX decisions

### I'm writing XAML/C# code
→ Read: **UI_IMPLEMENTATION_PATTERNS.md**
- XAML pattern examples
- Data binding implementations
- Navigation code
- Custom controls

### I need to understand navigation
→ Read: **UI_UX_PLAN.md** (Section 2) + **SCREEN_WIREFRAMES.md** (Navigation Maps)
- Shell structure
- Route definitions
- Flow diagrams

---

## Screen Quick Links

**By Purpose:**

**Learning Workflows:**
- Dashboard/Home → StudyMaterials → TopicDetails → LessonViewer → Quiz → Results

**User Management:**
- Profile → Settings → Edit Profile

**Analytics:**
- Progress/Analytics → View charts and achievements

**Browse Content:**
- StudyMaterials → Search/Filter → TopicDetails

---

## Design System Quick Facts

### Colors
```
Primary:   #007AFF (Blue) - Buttons, links
Success:   #34C759 (Green) - Completion, progress
Warning:   #FF9500 (Orange) - Stars, attention
Error:     #FF3B30 (Red) - Errors, delete actions
Neutral:   #333333-#F2F2F7 (Dark → Light)
```

### Typography
```
Headline (24px, Bold)      → Page titles
Title (20px, SemiBold)     → Section headers
Body (14-16px, Regular)    → Content, descriptions
Label (12px, Medium)       → Buttons, stats
Caption (11px, Regular)    → Helper text
```

### Spacing
```
Screen padding: 16px        (most common)
Card padding: 16px          (frame content)
Element spacing: 8-12px     (between items)
Component spacing: lg (24px) or xl (32px) for sections
```

### Touch Targets
```
Minimum: 48x48px
Optimal: 56-64px
Icons: 24x24px (with larger touch area)
Spacing between: 8px minimum
```

---

## Navigation Structure Cheat Sheet

```
┌─ Home (Dashboard)
│  └─ Quick actions, progress, activity
│
├─ Study Materials
│  └─ Topics List
│     └─ Topic Details
│        ├─ Overview
│        ├─ Learn (→ Lesson Viewer)
│        ├─ Exercises
│        └─ Resources
│           └─ Quiz (→ Results)
│
├─ Progress (Analytics)
│  └─ Charts, achievements, milestones
│
└─ Profile
   ├─ User info
   ├─ Edit Profile
   ├─ Learning Settings
   ├─ Account Settings
   └─ Logout
```

---

## Implementation Checklist

### Phase 1: Foundation (Week 1-2)
- [ ] Review all UI/UX documentation
- [ ] Set up color resources in App.xaml
- [ ] Create AppShell with 4 tabs
- [ ] Create HomePage layout structure
- [ ] Create StudyMaterialsPage with list
- [ ] Create button styles
- [ ] Test navigation between tabs

### Phase 2: Content Pages (Week 3-4)
- [ ] Create TopicDetailsPage with tabs
- [ ] Create LessonViewerPage
- [ ] Create QuizPage
- [ ] Create ResultsPage
- [ ] Implement page-to-page navigation
- [ ] Add query parameter handling
- [ ] Bind sample data

### Phase 3: Features (Week 5-6)
- [ ] Implement search functionality
- [ ] Add category filtering
- [ ] Create custom ProgressRingControl
- [ ] Implement loading states
- [ ] Add error handling
- [ ] Create achievement badges

### Phase 4: Polish (Week 7-8)
- [ ] Add animations
- [ ] Implement pull-to-refresh
- [ ] Optimize images
- [ ] Test responsive layouts
- [ ] Accessibility review
- [ ] Performance optimization

---

## Key Files to Create

### Views (XAML)
```
Views/
├── HomePage.xaml                    → Dashboard
├── StudyMaterialsPage.xaml          → Topics list
├── TopicDetailsPage.xaml            → Topic with tabs
├── LessonViewerPage.xaml            → Content viewer
├── QuizPage.xaml                    → Quiz/Assessment
├── ResultsPage.xaml                 → Quiz results
├── LearningProgressPage.xaml        → Analytics
├── ProfilePage.xaml                 → User profile
└── SettingsPage.xaml                → Settings
```

### ViewModels (C#)
```
ViewModels/
├── HomePageViewModel.cs
├── StudyMaterialsViewModel.cs
├── TopicDetailsViewModel.cs
├── LessonViewerViewModel.cs
├── QuizViewModel.cs
├── ResultsViewModel.cs
├── LearningProgressViewModel.cs
├── ProfileViewModel.cs
└── SettingsViewModel.cs
```

### Models (C#)
```
Models/
├── StudyTopic.cs
├── Lesson.cs
├── Quiz.cs
├── Question.cs
├── Answer.cs
├── Achievement.cs
├── DailyProgress.cs
└── TopicPerformance.cs
```

### Controls (Custom XAML)
```
Controls/
├── ProgressRingControl.xaml
├── ExpandableSection.xaml
└── StatCard.xaml
```

### Resources (XAML)
```
Resources/
├── Styles/
│   ├── Styles.xaml
│   ├── ButtonStyles.xaml
│   ├── TextStyles.xaml
│   └── ColorResources.xaml
├── Converters/
│   ├── PercentageConverter.cs
│   ├── BoolToColorConverter.cs
│   └── (others)
└── Images/
    ├── icons/
    ├── banners/
    └── achievements/
```

---

## Common Questions & Answers

### Q: Why 4 tabs instead of hamburger menu?
A: Material Design 3 favors bottom tabs for primary navigation. 4 tabs fit mobile screen width, each clearly distinct.

### Q: How do I handle the sticky action bar?
A: Use Grid with 2 rows: Row 1 (*) for content, Row 2 (Auto) for actions. See PATTERN 1 in UI_IMPLEMENTATION_PATTERNS.md

### Q: What about landscape orientation?
A: Responsive layouts adapt column counts. Phone: 1 col, Tablet: 2-3 cols. Use ResponsiveHelper class.

### Q: How do I bind data to the progress ring?
A: Use custom ProgressRingControl with Progress bindable property. See code example in UI_IMPLEMENTATION_PATTERNS.md

### Q: What's the navigation pattern for quiz?
A: Shell routing: `Quiz?topicId={id}` → Questions loop → Results page. See SCREEN 5 in SCREEN_WIREFRAMES.md

### Q: How do I implement search?
A: SearchBar with SearchCommand binding. Filter locally or use debounce for API calls. See PATTERN 2 in UI_IMPLEMENTATION_PATTERNS.md

### Q: Where do I put custom styles?
A: Resources/Styles/ folder with separate files for colors, buttons, text. Import in App.xaml

### Q: How do I handle loading states?
A: ActivityIndicator with IsRunning binding. Show with IsBusy property in ViewModel.

---

## Color Swatches

### For Design Tools (Figma, Sketch)
```
Primary:      RGB(0, 122, 255)    / HEX #007AFF
Success:      RGB(52, 199, 89)    / HEX #34C759
Warning:      RGB(255, 149, 0)    / HEX #FF9500
Error:        RGB(255, 59, 48)    / HEX #FF3B30
Dark Gray:    RGB(51, 51, 51)     / HEX #333333
Medium Gray:  RGB(209, 209, 214)  / HEX #D1D1D6
Light Gray:   RGB(242, 242, 247)  / HEX #F2F2F7
White:        RGB(255, 255, 255)  / HEX #FFFFFF
```

---

## Responsive Breakpoints

```
Phone (Vertical):          320 - 480px
Phone Large (Vertical):    481 - 767px
Tablet (All):              768px+

Grid Columns:
  Phone:   1 column
  Tablet:  2-3 columns

Touch Targets:
  Minimum: 48x48dp
  Optimal: 56x56dp

Font Scaling:
  Base: 360px width
  Formula: size * (screenWidth / 360)
```

---

## Accessibility Checklist

- [ ] All buttons minimum 48x48px
- [ ] Text contrast 4.5:1 (normal), 3:1 (large)
- [ ] Minimum font size 12px
- [ ] Line height 1.4x-1.6x
- [ ] All images have alt text
- [ ] Form inputs have labels
- [ ] Focus indicators visible
- [ ] Keyboard navigation works
- [ ] No color-only information
- [ ] Screen reader compatible

---

## Resources & References

### MAUI Documentation
- https://learn.microsoft.com/dotnet/maui
- https://learn.microsoft.com/dotnet/maui/xaml/bindings/mvvm

### Material Design 3
- https://m3.material.io
- https://material.io/design

### Accessibility
- https://www.w3.org/WAI/WCAG21/quickref/
- https://developer.android.com/guide/topics/ui/accessibility

### Code Examples
- See UI_IMPLEMENTATION_PATTERNS.md for production-ready samples

---

## Quick Commands

### Build & Run
```bash
cd D:\Code\claude\SharpReady

# Build
dotnet build -f net9.0-android

# Run on emulator
dotnet run -f net9.0-android

# Restore packages
dotnet restore
```

### Create New Page
1. Create `Views/MyPage.xaml`
2. Create `ViewModels/MyPageViewModel.cs`
3. Register in `MauiProgram.cs`
4. Add route to `AppShell.xaml`
5. Bind ViewModel in code-behind

---

## Contact & Support

For detailed information on any topic:

1. **Design Questions** → See UI_UX_PLAN.md
2. **Screen Layout** → See SCREEN_WIREFRAMES.md
3. **Code Implementation** → See UI_IMPLEMENTATION_PATTERNS.md
4. **Architecture** → See ARCHITECTURE.md
5. **Project Setup** → See README.md or GETTING_STARTED.md

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Mar 5, 2026 | Initial UI/UX design complete |
| - | - | - |

---

**Last Updated**: March 5, 2026
**Status**: Complete - Ready for Implementation
**Project**: SharpReady (MAUI Android)

Start with **UI_UX_SUMMARY.md** for a quick overview, then dive into specific documents as needed!
