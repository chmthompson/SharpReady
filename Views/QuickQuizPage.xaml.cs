using SharpReady.ViewModels;

namespace SharpReady.Views;

public partial class QuickQuizPage : ContentPage
{
    private readonly QuickQuizViewModel _vm;

    public QuickQuizPage(QuickQuizViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LaunchAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.ResetLaunched();
    }
}
