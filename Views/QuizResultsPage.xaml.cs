using SharpReady.ViewModels;

namespace SharpReady.Views;

[QueryProperty(nameof(SessionId), "SessionId")]
[QueryProperty(nameof(TopicId), "TopicId")]
public partial class QuizResultsPage : ContentPage
{
    private readonly QuizResultsViewModel _vm;

    public int SessionId { set => _vm.SessionId = value; }
    public int TopicId { set => _vm.TopicId = value; }

    public QuizResultsPage(QuizResultsViewModel vm)
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
