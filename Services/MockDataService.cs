namespace DotNetStudyAssistant.Services;

/// <summary>
/// Mock data service for development and testing
/// Replace with actual SQLite or REST API implementation
/// </summary>
public class MockDataService : IDataService
{
    private readonly Dictionary<Type, List<object>> _data = new();

    public MockDataService()
    {
        InitializeTestData();
    }

    public Task<T?> GetItemAsync<T>(int id) where T : class
    {
        if (!_data.ContainsKey(typeof(T)))
            return Task.FromResult<T?>(null);

        var item = _data[typeof(T)].FirstOrDefault(x =>
        {
            var idProp = x.GetType().GetProperty("Id");
            return idProp?.GetValue(x)?.Equals(id) ?? false;
        });

        return Task.FromResult(item as T);
    }

    public Task<IEnumerable<T>> GetItemsAsync<T>() where T : class
    {
        if (!_data.ContainsKey(typeof(T)))
            return Task.FromResult(Enumerable.Empty<T>());

        return Task.FromResult(_data[typeof(T)].Cast<T>());
    }

    public Task<int> SaveItemAsync<T>(T item) where T : class
    {
        if (!_data.ContainsKey(typeof(T)))
            _data[typeof(T)] = new List<object>();

        _data[typeof(T)].Add(item);
        return Task.FromResult(1);
    }

    public Task<bool> DeleteItemAsync<T>(int id) where T : class
    {
        if (!_data.ContainsKey(typeof(T)))
            return Task.FromResult(false);

        var item = _data[typeof(T)].FirstOrDefault(x =>
        {
            var idProp = x.GetType().GetProperty("Id");
            return idProp?.GetValue(x)?.Equals(id) ?? false;
        });

        if (item != null)
        {
            _data[typeof(T)].Remove(item);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private void InitializeTestData()
    {
        // Add test data here as needed
    }
}
