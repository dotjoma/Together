using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto);
    Task<string> UploadProfilePictureAsync(Guid userId, byte[] imageData, string fileName);
}
