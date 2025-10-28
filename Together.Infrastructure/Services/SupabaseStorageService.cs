using Microsoft.Extensions.Configuration;
using Together.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Together.Infrastructure.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly IConfiguration _configuration;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _bucketName = "profile-pictures";
    private const int MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB

    public SupabaseStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        _supabaseUrl = _configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL not configured");
        _supabaseKey = _configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase Key not configured");
    }

    public async Task<string> UploadProfilePictureAsync(byte[] imageData, string fileName, Guid userId)
    {
        if (imageData == null || imageData.Length == 0)
        {
            throw new ArgumentException("Image data cannot be empty", nameof(imageData));
        }

        if (imageData.Length > MaxFileSizeBytes)
        {
            imageData = await CompressImageAsync(imageData, MaxFileSizeBytes);
        }

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = $"profiles/{uniqueFileName}";

        // In a real implementation, this would upload to Supabase Storage
        // For now, we'll simulate by returning a URL
        var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{filePath}";

        // TODO: Implement actual Supabase storage upload
        // var client = new SupabaseClient(_supabaseUrl, _supabaseKey);
        // await client.Storage.From(_bucketName).Upload(imageData, filePath);

        return publicUrl;
    }

    public async Task<bool> DeleteProfilePictureAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return false;
        }

        // Extract file path from URL
        var uri = new Uri(fileUrl);
        var segments = uri.Segments;
        var filePath = string.Join("", segments.Skip(segments.Length - 2));

        // TODO: Implement actual Supabase storage deletion
        // var client = new SupabaseClient(_supabaseUrl, _supabaseKey);
        // await client.Storage.From(_bucketName).Remove(new[] { filePath });

        return await Task.FromResult(true);
    }

    public async Task<byte[]> CompressImageAsync(byte[] imageData, int maxSizeInBytes)
    {
        using var inputStream = new MemoryStream(imageData);
        using var image = await Image.LoadAsync(inputStream);

        // Resize if too large
        if (image.Width > 800 || image.Height > 800)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(800, 800),
                Mode = ResizeMode.Max
            }));
        }

        // Compress with quality adjustment
        var quality = 85;
        byte[] compressedData;

        do
        {
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = quality });
            compressedData = outputStream.ToArray();

            if (compressedData.Length <= maxSizeInBytes || quality <= 50)
            {
                break;
            }

            quality -= 10;
        } while (true);

        return compressedData;
    }

    public async Task<string> UploadImageAsync(string filePath, string folder)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Image file not found", filePath);
        }

        var imageData = await File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var storagePath = $"{folder}/{uniqueFileName}";

        // Compress if needed
        if (imageData.Length > 5 * 1024 * 1024) // 5MB
        {
            imageData = await CompressImageAsync(imageData, 5 * 1024 * 1024);
        }

        // In a real implementation, this would upload to Supabase Storage
        var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{storagePath}";

        // TODO: Implement actual Supabase storage upload
        // var client = new SupabaseClient(_supabaseUrl, _supabaseKey);
        // await client.Storage.From(_bucketName).Upload(imageData, storagePath);

        return publicUrl;
    }

    public async Task<bool> DeleteImageAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return false;
        }

        // Extract file path from URL
        try
        {
            var uri = new Uri(fileUrl);
            var segments = uri.Segments;
            var filePath = string.Join("", segments.Skip(segments.Length - 2));

            // TODO: Implement actual Supabase storage deletion
            // var client = new SupabaseClient(_supabaseUrl, _supabaseKey);
            // await client.Storage.From(_bucketName).Remove(new[] { filePath });

            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
    }
}
