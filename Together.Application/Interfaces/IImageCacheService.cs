using System;
using System.Threading.Tasks;

namespace Together.Application.Interfaces
{
    /// <summary>
    /// Service for lazy loading and caching images
    /// Returns byte arrays that can be converted to images in the presentation layer
    /// </summary>
    public interface IImageCacheService
    {
        /// <summary>
        /// Loads an image from URL with caching and returns byte array
        /// </summary>
        Task<byte[]> LoadImageAsync(string url);

        /// <summary>
        /// Preloads images in the background
        /// </summary>
        Task PreloadImagesAsync(params string[] urls);

        /// <summary>
        /// Clears the image cache
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Gets the cache size in bytes
        /// </summary>
        long GetCacheSize();
    }
}
