using System.Collections.ObjectModel;
using System.Windows.Input;
using DotNetStudyAssistant.Models;
using DotNetStudyAssistant.Models.Enums;
using DotNetStudyAssistant.Services;
using DotNetStudyAssistant.Utilities;

namespace DotNetStudyAssistant.ViewModels;

public class TopicDetailViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private Topic? _topic;
    private double _mastery;
    private int _questionCount;
    private int _defaultCount = 10;
    private DifficultyLevel _selectedDifficulty = DifficultyLevel.Mid;
    private bool _timerEnabled;
    private bool _isBusy;

    public Topic? Topic { get => _topic; set => SetProperty(ref _topic, value); }
    public double Mastery { get => _mastery; set => SetProperty(ref _mastery, value); }
    public int QuestionCount { get => _questionCount; set => SetProperty(ref _questionCount, value); }
    public int DefaultCount { get => _defaultCount; set => SetProperty(ref _defaultCount, value); }
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
