using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Together.Application.Interfaces;

namespace Together.Infrastructure.Services
{
    /// <summary>
    /// In-memory cache implementation for user session data and frequently accessed items
    /// </summary>
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache;
        private readonly TimeSpan _defaultExpiration;

        public MemoryCacheService()
        {
            _cache = new ConcurrentDictionary<string, CacheEntry>();
            _defaultExpiration = TimeSpan.FromMinutes(15);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    return (T)entry.Value;
                }
                
                // Remove expired entry
                _cache.TryRemove(key, out _);
            }

            var value = await factory();
            Set(key, value, expiration);
            return value;
        }

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    return (T)entry.Value;
                }
                
                // Remove expired entry
                _cache.TryRemove(key, out _);
            }

            return default!;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var expirationTime = expiration ?? _defaultExpiration;
            var entry = new CacheEntry
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(expirationTime)
            };

            _cache.AddOrUpdate(key, entry, (k, oldValue) => entry);
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool Exists(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.IsExpired)
                {
                    return true;
                }
                
                // Remove expired entry
                _cache.TryRemove(key, out _);
            }

            return false;
        }

        private class CacheEntry
        {
            public object Value { get; set; } = null!;
            public DateTime ExpiresAt { get; set; }
            public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        }
    }
}
