using System.Windows.Input;
using SharpReady.Services;
using SharpReady.Utilities;

namespace SharpReady.ViewModels;

public class QuickQuizViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private bool _isBusy;
    private bool _hasLaunched;

    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public QuickQuizViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
    }

    public void ResetLaunched() => _hasLaunched = false;

    public async Task LaunchAsync()
    {
        if (_hasLaunched) return;
        _hasLaunched = true;

        IsBusy = true;
        try
        {
            // TopicId = 0 signals the quiz to draw randomly from the full question bank
            await _navigationService.NavigateToAsync("Quiz", new Dictionary<string, object>
            {
                ["TopicId"] = 0,
                ["Count"] = 10
            });
        }
        finally
        {
            IsBusy = false;
        }
    }
}
