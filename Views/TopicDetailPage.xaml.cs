using SharpReady.ViewModels;

namespace SharpReady.Views;

[QueryProperty(nameof(TopicId), "TopicId")]
public partial class TopicDetailPage : ContentPage
{
    private readonly TopicDetailViewModel _vm;

    public int TopicId
    {
        set
        {
            _vm.TopicId = value;
            _ = _vm.LoadAsync();
        }
    }

    public TopicDetailPage(TopicDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }
}
