using System.Windows.Input;
using DotNetStudyAssistant.Models.Enums;
using DotNetStudyAssistant.Utilities;

namespace DotNetStudyAssistant.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private DifficultyLevel _targetRole = DifficultyLevel.Senior;
    private DifficultyLevel _currentLevel = DifficultyLevel.Mid;
    private int _defaultQuestionCount = 10;
    private bool _timerEnabled;
    private DifficultyLevel _defaultDifficulty = DifficultyLevel.Mid;

    public DifficultyLevel TargetRole { get => _targetRole; set => SetProperty(ref _targetRole, value); }
    public DifficultyLevel CurrentLevel { get => _currentLevel; set => SetProperty(ref _currentLevel, value); }
    public int DefaultQuestionCount { get => _defaultQuestionCount; set => SetProperty(ref _defaultQuestionCount, value); }
    public bool TimerEnabled { get => _timerEnabled; set => SetProperty(ref _timerEnabled, value); }
    public DifficultyLevel DefaultDifficulty { get => _defaultDifficulty; set => SetProperty(ref _defaultDifficulty, value); }

    public DifficultyLevel[] Levels { get; } = Enum.GetValues<DifficultyLevel>();
    public int[] QuestionCounts { get; } = [5, 10, 15, 20];

    public ICommand SaveCommand { get; }

    public SettingsViewModel()
    {
        SaveCommand = new Command(Save);
    }

    private void Save()
    {
        Preferences.Set("TargetRole", (int)TargetRole);
        Preferences.Set("CurrentLevel", (int)CurrentLevel);
        Preferences.Set("DefaultQuestionCount", DefaultQuestionCount);
        Preferences.Set("TimerEnabled", TimerEnabled);
        Preferences.Set("DefaultDifficulty", (int)DefaultDifficulty);
    }

    public void Load()
    {
        TargetRole = (DifficultyLevel)Preferences.Get("TargetRole", (int)DifficultyLevel.Senior);
        CurrentLevel = (DifficultyLevel)Preferences.Get("CurrentLevel", (int)DifficultyLevel.Mid);
        DefaultQuestionCount = Preferences.Get("DefaultQuestionCount", 10);
        TimerEnabled = Preferences.Get("TimerEnabled", false);
        DefaultDifficulty = (DifficultyLevel)Preferences.Get("DefaultDifficulty", (int)DifficultyLevel.Mid);
    }
}
