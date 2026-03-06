using System.Windows.Input;
using DotNetStudyAssistant.Models;
using DotNetStudyAssistant.Models.Enums;
using DotNetStudyAssistant.Services;
using DotNetStudyAssistant.Utilities;

namespace DotNetStudyAssistant.ViewModels;

public class QuizViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private List<Question> _questions = [];
    private int _currentIndex;
    private string _selectedAnswer = string.Empty;
    private bool _isAnswerSubmitted;
    private bool _isCorrect;
    private bool _isBusy;
    private int _timeRemaining;
    private IDispatcherTimer? _timer;

    public Question? CurrentQuestion => _questions.Count > 0 ? _questions[_currentIndex] : null;
    public int CurrentNumber => _currentIndex + 1;
    public int TotalQuestions => _questions.Count;
    public double Progress => TotalQuestions > 0 ? (double)CurrentNumber / TotalQuestions : 0;

    public string SelectedAnswer { get => _selectedAnswer; set => SetProperty(ref _selectedAnswer, value); }
    public bool IsAnswerSubmitted { get => _isAnswerSubmitted; set => SetProperty(ref _isAnswerSubmitted, value); }
    public bool IsCorrect { get => _isCorrect; set => SetProperty(ref _isCorrect, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public int TimeRemaining { get => _timeRemaining; set => SetProperty(ref _timeRemaining, value); }
    public bool TimerEnabled { get; set; }

    public List<QuizAnswer> Answers { get; } = [];
    public QuizSession Session { get; private set; } = new();

    public int TopicId { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Mid;
    public int Count { get; set; } = 10;

    public ICommand LoadCommand { get; }
    public ICommand SelectAnswerCommand { get; }
    public ICommand NextCommand { get; }

    public QuizViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
        LoadCommand = new Command(async () => await LoadAsync());
        SelectAnswerCommand = new Command<string>(SelectAnswer);
        NextCommand = new Command(async () => await NextAsync(), () => IsAnswerSubmitted);
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            _questions = await _quizService.GetQuestionsAsync(TopicId, Difficulty, Count);
            _currentIndex = 0;
            Answers.Clear();
            Session = new QuizSession { TopicId = TopicId, StartTime = DateTime.UtcNow };
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentNumber));
            OnPropertyChanged(nameof(Progress));

            if (TimerEnabled)
                StartTimer();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SelectAnswer(string answer)
    {
        if (IsAnswerSubmitted) return;

        SelectedAnswer = answer;
        IsAnswerSubmitted = true;
        IsCorrect = answer == CurrentQuestion?.CorrectAnswer;

        Answers.Add(new QuizAnswer
        {
            QuestionId = CurrentQuestion!.Id,
            SelectedAnswer = answer,
            IsCorrect = IsCorrect
        });

        _timer?.Stop();
        ((Command)NextCommand).ChangeCanExecute();
    }

    private async Task NextAsync()
    {
        if (_currentIndex < _questions.Count - 1)
        {
            _currentIndex++;
            SelectedAnswer = string.Empty;
            IsAnswerSubmitted = false;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentNumber));
            OnPropertyChanged(nameof(Progress));
            ((Command)NextCommand).ChangeCanExecute();

            if (TimerEnabled)
                StartTimer();
        }
        else
        {
            await FinishQuizAsync();
        }
    }

    private async Task FinishQuizAsync()
    {
        Session.EndTime = DateTime.UtcNow;
        Session.Answers.AddRange(Answers);

        await _quizService.SaveSessionAsync(Session);
        await _quizService.UpdateProgressAsync(TopicId, Session);

        await _navigationService.NavigateToAsync("QuizResults", new Dictionary<string, object>
        {
            ["SessionId"] = Session.Id,
            ["TopicId"] = TopicId
        });
    }

    private void StartTimer(int seconds = 60)
    {
        _timer?.Stop();
        TimeRemaining = seconds;
        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, _) =>
        {
            TimeRemaining--;
            if (TimeRemaining <= 0)
            {
                _timer.Stop();
                if (!IsAnswerSubmitted)
                    SelectAnswer(string.Empty); // time's up — mark wrong
            }
        };
        _timer.Start();
    }
}
