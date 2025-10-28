using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;

    public ProfileService(IUserRepository userRepository, IStorageService storageService)
    {
        _userRepository = userRepository;
        _storageService = storageService;
    }

    public async Task<ProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(user), userId);
        }

        // Get follower and following counts
        // Note: This requires the navigation properties to be loaded
        var followerCount = user.Followers?.Count ?? 0;
        var followingCount = user.Following?.Count ?? 0;

        return new ProfileDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.ProfilePictureUrl,
            user.Bio,
            user.Visibility,
            followerCount,
            followingCount,
            user.CreatedAt
        );
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(user), userId);
        }

        // Validate bio length
        if (updateDto.Bio != null && updateDto.Bio.Length > 500)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(updateDto.Bio), new[] { "Bio cannot exceed 500 characters" } }
            });
        }

        user.UpdateProfile(updateDto.Bio, updateDto.ProfilePictureUrl, updateDto.Visibility);
        await _userRepository.UpdateAsync(user);

        return await GetProfileAsync(userId);
    }

    public async Task<string> UploadProfilePictureAsync(Guid userId, byte[] imageData, string fileName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException(nameof(user), userId);
        }

        // Validate file size (2MB max)
        const int maxFileSize = 2 * 1024 * 1024;
        if (imageData.Length > maxFileSize)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "ProfilePicture", new[] { "Profile picture must be less than 2MB" } }
            });
        }

        // Validate file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "ProfilePicture", new[] { "Profile picture must be JPG or PNG format" } }
            });
        }

        // Delete old profile picture if exists
        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
        {
            await _storageService.DeleteProfilePictureAsync(user.ProfilePictureUrl);
        }

        // Upload new profile picture
        var profilePictureUrl = await _storageService.UploadProfilePictureAsync(imageData, fileName, userId);

        // Update user profile with new picture URL
        user.UpdateProfile(user.Bio, profilePictureUrl, user.Visibility);
        await _userRepository.UpdateAsync(user);

        return profilePictureUrl;
    }
}
