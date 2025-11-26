using System.Runtime.Caching;
using TfsViewer.Core.Contracts;

namespace TfsViewer.Core.Services;

/// <summary>
/// In-memory caching service with TTL support
/// </summary>
public class CacheService : ICacheService
{
    private readonly MemoryCache _cache = MemoryCache.Default;

    public void Set<T>(string key, T value, TimeSpan ttl)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.Add(ttl)
        };

        _cache.Set(key, value, policy);
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        var value = _cache.Get(key);
        return value is T typedValue ? typedValue : default;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        value = Get<T>(key);
        return value != null;
    }

    public void Remove(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            _cache.Remove(key);
        }
    }

    public void Clear()
    {
        foreach (var item in _cache)
        {
            _cache.Remove(item.Key);
        }
    }

    public bool Contains(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _cache.Contains(key);
    }
}
