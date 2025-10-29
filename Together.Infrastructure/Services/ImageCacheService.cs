using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Together.Application.Interfaces;

namespace Together.Infrastructure.Services
{
    /// <summary>
    /// Image caching service with lazy loading support
    /// Returns byte arrays that can be converted to images in the presentation layer
    /// </summary>
    public class ImageCacheService : IImageCacheService
    {
        private readonly ConcurrentDictionary<string, byte[]> _imageCache;
        private readonly ConcurrentDictionary<string, Task<byte[]>> _loadingTasks;
        private readonly HttpClient _httpClient;
        private const long MaxCacheSizeBytes = 100 * 1024 * 1024; // 100 MB

        public ImageCacheService()
        {
            _imageCache = new ConcurrentDictionary<string, byte[]>();
            _loadingTasks = new ConcurrentDictionary<string, Task<byte[]>>();
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<byte[]> LoadImageAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null!;
            }

            // Check if already cached
            if (_imageCache.TryGetValue(url, out var cachedImage))
            {
                return cachedImage;
            }

            // Check if already loading
            if (_loadingTasks.TryGetValue(url, out var loadingTask))
            {
                return await loadingTask;
            }

            // Start loading
            var task = LoadImageInternalAsync(url);
            _loadingTasks.TryAdd(url, task);

            try
            {
                var imageData = await task;
                if (imageData != null)
                {
                    _imageCache.TryAdd(url, imageData);
                }
                return imageData;
            }
            finally
            {
                _loadingTasks.TryRemove(url, out _);
            }
        }

        public async Task PreloadImagesAsync(params string[] urls)
        {
            var tasks = urls
                .Where(url => !string.IsNullOrWhiteSpace(url) && !_imageCache.ContainsKey(url))
                .Select(url => LoadImageAsync(url));

            await Task.WhenAll(tasks);
        }

        public void ClearCache()
        {
            _imageCache.Clear();
            _loadingTasks.Clear();
        }

        public long GetCacheSize()
        {
            // Calculate actual cache size from byte arrays
            long totalSize = 0;
            foreach (var imageData in _imageCache.Values)
            {
                if (imageData != null)
                {
                    totalSize += imageData.Length;
                }
            }
            return totalSize;
        }

        private async Task<byte[]> LoadImageInternalAsync(string url)
        {
            try
            {
                // Check if it's a local file
                if (File.Exists(url))
                {
                    return await File.ReadAllBytesAsync(url);
                }

                // Load from URL
                return await _httpClient.GetByteArrayAsync(url);
            }
            catch (Exception)
            {
                // Return null on error - caller should handle
                return null!;
            }
        }
    }
}
