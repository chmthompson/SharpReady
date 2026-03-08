using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpReady.Models;
using SharpReady.Services;
using SharpReady.Utilities;

namespace SharpReady.ViewModels;

public class QuizResultsViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private QuizSession? _session;
    private Topic? _topic;
    private bool _isBusy;

    public QuizSession? Session { get => _session; set => SetProperty(ref _session, value); }
    public Topic? Topic { get => _topic; set => SetProperty(ref _topic, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ObservableCollection<WrongAnswerItem> WrongAnswers { get; } = [];

    public int SessionId { get; set; }
    public int TopicId { get; set; }

    public string ScoreLabel => Session != null ? $"{Session.Score}/{Session.TotalQuestions}" : "-";
    public string PercentLabel => Session != null ? $"{Session.ScorePercent:F0}%" : "-";
    public bool Passed => Session?.ScorePercent >= 70;
    public string TopicDisplayName => Topic?.Name ?? "Quick Quiz";

    public ICommand LoadCommand { get; }
    public ICommand PracticeAgainCommand { get; }
    public ICommand BackToTopicsCommand { get; }

    public QuizResultsViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
        LoadCommand = new Command(async () => await LoadAsync());
        PracticeAgainCommand = new Command(async () =>
            await _navigationService.NavigateToAsync("TopicDetail",
                new Dictionary<string, object> { ["TopicId"] = TopicId }));
        BackToTopicsCommand = new Command(async () =>
            await _navigationService.NavigateToAsync("//Practice"));
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var sessions = await _quizService.GetSessionsAsync(TopicId);
            Session = sessions.FirstOrDefault(s => s.Id == SessionId);

            var topics = await _quizService.GetTopicsAsync();
            Topic = topics.FirstOrDefault(t => t.Id == TopicId);

            OnPropertyChanged(nameof(ScoreLabel));
            OnPropertyChanged(nameof(PercentLabel));
            OnPropertyChanged(nameof(Passed));
            OnPropertyChanged(nameof(TopicDisplayName));

            WrongAnswers.Clear();
            if (Session != null)
            {
                // TopicId == 0 means Quick Quiz; pass 0 to search the full question bank
                var questions = await _quizService.GetQuestionsAsync(TopicId == 0 ? 0 : TopicId, count: 1000);
                foreach (var answer in Session.Answers.Where(a => !a.IsCorrect))
                {
                    var q = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (q != null)
                        WrongAnswers.Add(new WrongAnswerItem
                        {
                            QuestionText = q.Text,
                            YourAnswer = string.IsNullOrEmpty(answer.SelectedAnswer) ? "(Time's up)" : answer.SelectedAnswer,
                            CorrectAnswer = q.CorrectAnswer,
                            Explanation = q.Explanation,
                            ExampleCode = q.ExampleCode
                        });
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class WrongAnswerItem : BaseViewModel
{
    private bool _showCode;

    public string QuestionText { get; set; } = string.Empty;
    public string YourAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string? ExampleCode { get; set; }

    public bool HasExampleCode => ExampleCode != null;
    public bool ShowCode
    {
        get => _showCode;
        set { SetProperty(ref _showCode, value); OnPropertyChanged(nameof(ExampleCodeLinkText)); }
    }
    public string ExampleCodeLinkText => ShowCode ? "▴ Hide example" : "▾ Show example code";

    public ICommand ToggleCodeCommand { get; }

    public WrongAnswerItem()
    {
        ToggleCodeCommand = new Command(() => ShowCode = !ShowCode);
    }
}
