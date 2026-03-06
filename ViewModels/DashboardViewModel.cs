using System.Collections.ObjectModel;
using System.Windows.Input;
using DotNetStudyAssistant.Models;
using DotNetStudyAssistant.Services;
using DotNetStudyAssistant.Utilities;

namespace DotNetStudyAssistant.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private double _readinessScore;
    private int _questionsAnswered;
    private int _topicsStarted;
    private bool _isBusy;

    public double ReadinessScore { get => _readinessScore; set => SetProperty(ref _readinessScore, value); }
    public int QuestionsAnswered { get => _questionsAnswered; set => SetProperty(ref _questionsAnswered, value); }
    public int TopicsStarted { get => _topicsStarted; set => SetProperty(ref _topicsStarted, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ObservableCollection<TopicWithProgress> WeakestAreas { get; } = [];

    public ICommand LoadCommand { get; }
    public ICommand GoToPracticeCommand { get; }
    public ICommand QuickQuizCommand { get; }

    public DashboardViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
        LoadCommand = new Command(async () => await LoadAsync());
        GoToPracticeCommand = new Command(async () => await _navigationService.NavigateToAsync("//Practice"));
        QuickQuizCommand = new Command(async () => await _navigationService.NavigateToAsync("//Practice"));
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var topics = await _quizService.GetTopicsAsync();
            var allProgress = await _quizService.GetProgressAsync();
            var sessions = await _quizService.GetSessionsAsync();

            QuestionsAnswered = allProgress.Sum(p => p.QuestionsAnswered);
            TopicsStarted = allProgress.Count;

            ReadinessScore = allProgress.Count > 0
                ? allProgress.Average(p => p.MasteryPercent)
                : 0;

            WeakestAreas.Clear();
            var weak = topics
                .Select(t => new TopicWithProgress
                {
                    Topic = t,
                    Mastery = allProgress.FirstOrDefault(p => p.TopicId == t.Id)?.MasteryPercent ?? 0,
                    QuestionsAnswered = allProgress.FirstOrDefault(p => p.TopicId == t.Id)?.QuestionsAnswered ?? 0
                })
                .Where(x => x.Mastery < 60)
                .OrderBy(x => x.Mastery)
                .Take(3);

            foreach (var item in weak)
                WeakestAreas.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
