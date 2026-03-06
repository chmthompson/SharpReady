namespace DotNetStudyAssistant.Services;

/// <summary>
/// Implementation of navigation service using MAUI Shell routing
/// </summary>
public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters != null && parameters.Count > 0)
        {
            return Shell.Current.GoToAsync(CreateRoute(route, parameters));
        }

        return Shell.Current.GoToAsync(route);
    }

    public Task GoBackAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    public Task GoBackToRootAsync()
    {
        return Shell.Current.GoToAsync("//");
    }

    private string CreateRoute(string route, IDictionary<string, object> parameters)
    {
        var queryString = string.Empty;
        if (parameters.Count > 0)
        {
            queryString = "?" + string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        return route + queryString;
    }
}
