namespace Together.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadProfilePictureAsync(byte[] imageData, string fileName, Guid userId);
    Task<bool> DeleteProfilePictureAsync(string fileUrl);
    Task<byte[]> CompressImageAsync(byte[] imageData, int maxSizeInBytes);
}
