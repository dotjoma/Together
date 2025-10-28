namespace Together.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadProfilePictureAsync(byte[] imageData, string fileName, Guid userId);
    Task<bool> DeleteProfilePictureAsync(string fileUrl);
    Task<byte[]> CompressImageAsync(byte[] imageData, int maxSizeInBytes);
    Task<string> UploadImageAsync(string filePath, string folder);
    Task<bool> DeleteImageAsync(string fileUrl);
    Task<string> UploadFileAsync(Stream fileStream, string filePath, string contentType);
    Task<bool> DeleteFileAsync(string fileUrl);
}
