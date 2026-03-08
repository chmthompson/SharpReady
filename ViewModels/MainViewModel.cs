using System.Collections.ObjectModel;
using System.Windows.Input;
using SharpReady.Models;
using SharpReady.Services;
using SharpReady.Utilities;

namespace SharpReady.ViewModels;

/// <summary>
/// ViewModel for the main page
/// Demonstrates MVVM pattern with data binding
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    private int _count;
    private bool _isBusy;
    private string _title = "Welcome to SharpReady";
    private string _statusMessage = "Ready";
    private ObservableCollection<UserProfile> _users;

    public MainViewModel()
    {
        _dataService = new MockDataService();
        _navigationService = new NavigationService();
        _users = new ObservableCollection<UserProfile>();

        // Load data on initialization
        LoadDataCommand = new Command(async () => await LoadData());
        ResetCommand = new Command(Reset);
    }

    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<UserProfile> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand ResetCommand { get; }

    public void IncrementCounter()
    {
        Count++;
        StatusMessage = Count == 1 ? $"Clicked {Count} time" : $"Clicked {Count} times";
    }

    private async Task LoadData()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading data...";

            var users = await _dataService.GetItemsAsync<UserProfile>();
            Users = new ObservableCollection<UserProfile>(users);

            StatusMessage = $"Loaded {Users.Count} users";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Reset()
    {
        Count = 0;
        StatusMessage = "Reset complete";
    }
}
