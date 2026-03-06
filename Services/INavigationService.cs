namespace DotNetStudyAssistant.Services;

/// <summary>
/// Interface for navigation service
/// Handles routing and page navigation
/// </summary>
public interface INavigationService
{
    Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null);
    Task GoBackAsync();
    Task GoBackToRootAsync();
}
