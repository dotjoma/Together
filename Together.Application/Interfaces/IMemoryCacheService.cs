using System;
using System.Threading.Tasks;

namespace Together.Application.Interfaces
{
    /// <summary>
    /// Service for in-memory caching of frequently accessed data
    /// </summary>
    public interface IMemoryCacheService
    {
        /// <summary>
        /// Gets a cached value or computes it if not present
        /// </summary>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Gets a cached value
        /// </summary>
        T Get<T>(string key);

        /// <summary>
        /// Sets a cached value
        /// </summary>
        void Set<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Removes a cached value
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Clears all cached values
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks if a key exists in cache
        /// </summary>
        bool Exists(string key);
    }
}
