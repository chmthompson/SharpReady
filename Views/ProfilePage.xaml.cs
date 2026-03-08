namespace SharpReady.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    private async void OnGoBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
