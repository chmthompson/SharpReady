using SharpReady.ViewModels;

namespace SharpReady.Views;

public partial class TopicsPage : ContentPage
{
    private readonly TopicsViewModel _vm;

    public TopicsPage(TopicsViewModel vm)
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
