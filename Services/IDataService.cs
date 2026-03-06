namespace DotNetStudyAssistant.Services;

/// <summary>
/// Interface for data persistence service
/// Implement with SQLite, REST API, or cloud storage
/// </summary>
public interface IDataService
{
    Task<T?> GetItemAsync<T>(int id) where T : class;
    Task<IEnumerable<T>> GetItemsAsync<T>() where T : class;
    Task<int> SaveItemAsync<T>(T item) where T : class;
    Task<bool> DeleteItemAsync<T>(int id) where T : class;
}
