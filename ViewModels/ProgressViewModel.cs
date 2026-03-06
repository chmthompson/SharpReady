using System.Collections.ObjectModel;
using System.Windows.Input;
using DotNetStudyAssistant.Models;
using DotNetStudyAssistant.Models.Enums;
using DotNetStudyAssistant.Services;
using DotNetStudyAssistant.Utilities;

namespace DotNetStudyAssistant.ViewModels;

public class ProgressViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private double _overallReadiness;
    private DifficultyLevel _targetLevel = DifficultyLevel.Senior;
    private bool _isBusy;

    public double OverallReadiness { get => _overallReadiness; set => SetProperty(ref _overallReadiness, value); }
    public DifficultyLevel TargetLevel { get => _targetLevel; set { SetProperty(ref _targetLevel, value); _ = LoadAsync(); } }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public string ReadinessLabel => $"{TargetLevel}: {OverallReadiness:F0}% ready";

    public ObservableCollection<TopicMastery> TopicMasteries { get; } = [];
    public ObservableCollection<TopicMastery> NeedsWork { get; } = [];
    public ObservableCollection<QuizSession> RecentSessions { get; } = [];

    public DifficultyLevel[] Levels { get; } = Enum.GetValues<DifficultyLevel>();

    public ICommand LoadCommand { get; }

    public ProgressViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var topics = await _quizService.GetTopicsAsync();
            var progress = await _quizService.GetProgressAsync();
            var sessions = await _quizService.GetSessionsAsync();

            var masteries = topics.Select(t => new TopicMastery
            {
                Topic = t,
                Mastery = progress.FirstOrDefault(p => p.TopicId == t.Id)?.MasteryPercent ?? 0,
                QuestionsAnswered = progress.FirstOrDefault(p => p.TopicId == t.Id)?.QuestionsAnswered ?? 0,
                LastAttempted = progress.FirstOrDefault(p => p.TopicId == t.Id)?.LastAttempted
            }).OrderByDescending(m => m.Mastery).ToList();

            OverallReadiness = masteries.Count > 0 ? masteries.Average(m => m.Mastery) : 0;
            OnPropertyChanged(nameof(ReadinessLabel));

            TopicMasteries.Clear();
            foreach (var m in masteries)
                TopicMasteries.Add(m);

            NeedsWork.Clear();
            foreach (var m in masteries.Where(m => m.Mastery < 60).OrderBy(m => m.Mastery))
                NeedsWork.Add(m);

            RecentSessions.Clear();
            foreach (var s in sessions.Take(10))
                RecentSessions.Add(s);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class TopicMastery
{
    public Topic Topic { get; set; } = null!;
    public double Mastery { get; set; }
    public int QuestionsAnswered { get; set; }
    public DateTime? LastAttempted { get; set; }
    public string MasteryLabel => $"{Mastery:F0}%";
    public double MasteryProgress => Mastery / 100.0;
    public string LastAttemptedLabel => LastAttempted.HasValue
        ? LastAttempted.Value.ToLocalTime().ToString("MMM d")
        : "Not started";
}
