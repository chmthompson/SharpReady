using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpReady.Models;
using SharpReady.Models.Enums;
using SharpReady.Services;
using SharpReady.Utilities;

namespace SharpReady.ViewModels;

public class TopicDetailViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private Topic? _topic;
    private double _mastery;
    private int _questionCount;
    private int _selectedCountIndex = 1; // index 1 = 10 questions
    private DifficultyLevel _selectedDifficulty = DifficultyLevel.Mid;
    private bool _timerEnabled;
    private bool _isBusy;

    public Topic? Topic { get => _topic; set => SetProperty(ref _topic, value); }
    public double Mastery { get => _mastery; set => SetProperty(ref _mastery, value); }
    public int QuestionCount
    {
        get => _questionCount;
        set
        {
            SetProperty(ref _questionCount, value);
            OnPropertyChanged(nameof(AvailableCountOptions));
            if (SelectedCountIndex >= AvailableCountOptions.Count)
                SelectedCountIndex = AvailableCountOptions.Count - 1;
        }
    }
    private static readonly List<int> CountOptions = [5, 10, 15, 20];
    public List<int> AvailableCountOptions => CountOptions.Where(c => c <= _questionCount).ToList() is { Count: > 0 } opts ? opts : [Math.Min(5, _questionCount)];
    public int SelectedCountIndex { get => _selectedCountIndex; set => SetProperty(ref _selectedCountIndex, value); }
    public int DefaultCount => SelectedCountIndex >= 0 && SelectedCountIndex < AvailableCountOptions.Count ? AvailableCountOptions[SelectedCountIndex] : AvailableCountOptions.FirstOrDefault(5);
    public DifficultyLevel SelectedDifficulty { get => _selectedDifficulty; set => SetProperty(ref _selectedDifficulty, value); }
    public bool TimerEnabled { get => _timerEnabled; set => SetProperty(ref _timerEnabled, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ObservableCollection<QuizSession> RecentSessions { get; } = [];
    public DifficultyLevel[] Difficulties { get; } = Enum.GetValues<DifficultyLevel>();

    public ICommand LoadCommand { get; }
    public ICommand StartQuizCommand { get; }

    public int TopicId { get; set; }

    public TopicDetailViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
        LoadCommand = new Command(async () => await LoadAsync());
        StartQuizCommand = new Command(async () => await StartQuizAsync());
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var topics = await _quizService.GetTopicsAsync();
            Topic = topics.FirstOrDefault(t => t.Id == TopicId);

            var progress = await _quizService.GetTopicProgressAsync(TopicId);
            Mastery = progress?.MasteryPercent ?? 0;

            var questions = await _quizService.GetQuestionsAsync(TopicId, count: 100);
            QuestionCount = questions.Count;

            var sessions = await _quizService.GetSessionsAsync(TopicId);
            RecentSessions.Clear();
            foreach (var s in sessions.Take(5))
                RecentSessions.Add(s);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartQuizAsync()
    {
        await _navigationService.NavigateToAsync("Quiz", new Dictionary<string, object>
        {
            ["TopicId"] = TopicId,
            ["Difficulty"] = (int)SelectedDifficulty,
            ["Count"] = DefaultCount,
            ["TimerEnabled"] = TimerEnabled
        });
    }
}
