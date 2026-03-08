using SharpReady.ViewModels;

namespace SharpReady;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		if (BindingContext is MainViewModel viewModel)
		{
			viewModel.IncrementCounter();
			SemanticScreenReader.Announce(viewModel.StatusMessage);
		}
	}
}
