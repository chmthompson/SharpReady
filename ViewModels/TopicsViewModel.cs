using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpReady.Models;
using SharpReady.Models.Enums;
using SharpReady.Services;
using SharpReady.Utilities;

namespace SharpReady.ViewModels;

public class TopicsViewModel : BaseViewModel
{
    private readonly IQuizService _quizService;
    private readonly INavigationService _navigationService;

    private string _searchText = string.Empty;
    private CSharpCategory? _selectedCategory;
    private bool _isBusy;

    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); ApplyFilter(); }
    }

    public CSharpCategory? SelectedCategory
    {
        get => _selectedCategory;
        set { SetProperty(ref _selectedCategory, value); ApplyFilter(); }
    }

    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ObservableCollection<TopicWithProgress> Topics { get; } = [];

    public ICommand LoadCommand { get; }
    public ICommand SelectTopicCommand { get; }
    public ICommand ClearCategoryCommand { get; }
    public ICommand SetCategoryCommand { get; }

    private List<TopicWithProgress> _allTopics = [];

    public DifficultyLevel[] Difficulties { get; } = Enum.GetValues<DifficultyLevel>();
    public CSharpCategory?[] Categories { get; } =
        [null, .. Enum.GetValues<CSharpCategory>().Cast<CSharpCategory?>()];

    public TopicsViewModel(IQuizService quizService, INavigationService navigationService)
    {
        _quizService = quizService;
        _navigationService = navigationService;
        LoadCommand = new Command(async () => await LoadAsync());
        SelectTopicCommand = new Command<TopicWithProgress>(async t => await SelectTopicAsync(t));
        ClearCategoryCommand = new Command(() => SelectedCategory = null);
        SetCategoryCommand = new Command<string>(cat =>
        {
            if (Enum.TryParse<CSharpCategory>(cat, out var parsed))
                SelectedCategory = parsed;
        });
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var topics = await _quizService.GetTopicsAsync();
            var progress = await _quizService.GetProgressAsync();

            _allTopics = topics.Select(t => new TopicWithProgress
            {
                Topic = t,
                Mastery = progress.FirstOrDefault(p => p.TopicId == t.Id)?.MasteryPercent ?? 0,
                QuestionsAnswered = progress.FirstOrDefault(p => p.TopicId == t.Id)?.QuestionsAnswered ?? 0
            }).ToList();

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allTopics.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(t =>
                t.Topic.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Topic.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (SelectedCategory.HasValue)
            filtered = filtered.Where(t => t.Topic.Category == SelectedCategory.Value);

        Topics.Clear();
        foreach (var item in filtered)
            Topics.Add(item);
    }

    private async Task SelectTopicAsync(TopicWithProgress topic)
    {
        await _navigationService.NavigateToAsync("TopicDetail",
            new Dictionary<string, object> { ["TopicId"] = topic.Topic.Id });
    }
}

public class TopicWithProgress
{
    public Topic Topic { get; set; } = null!;
    public double Mastery { get; set; }
    public int QuestionsAnswered { get; set; }
    public string MasteryLabel => $"{Mastery:F0}%";
    public string CategoryLabel => Topic.Category.ToString();
}
