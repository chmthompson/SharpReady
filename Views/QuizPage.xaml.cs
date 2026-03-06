using DotNetStudyAssistant.Models.Enums;
using DotNetStudyAssistant.ViewModels;

namespace DotNetStudyAssistant.Views;

[QueryProperty(nameof(TopicId), "TopicId")]
[QueryProperty(nameof(Difficulty), "Difficulty")]
[QueryProperty(nameof(Count), "Count")]
[QueryProperty(nameof(TimerEnabled), "TimerEnabled")]
public partial class QuizPage : ContentPage
{
    private readonly QuizViewModel _vm;

    public int TopicId { set => _vm.TopicId = value; }
    public int Difficulty { set => _vm.Difficulty = (DifficultyLevel)value; }
    public int Count { set => _vm.Count = value; }
    public bool TimerEnabled { set => _vm.TimerEnabled = value; }

    public QuizPage(QuizViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
