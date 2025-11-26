namespace TfsViewer.Core.Contracts;

/// <summary>
/// Service interface for caching data with TTL
/// </summary>
public interface ICacheService
{
    void Set<T>(string key, T value, TimeSpan ttl);
    
    T? Get<T>(string key);
    
    bool TryGet<T>(string key, out T? value);
    
    void Remove(string key);
    
    void Clear();
    
    bool Contains(string key);
}
