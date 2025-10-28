using Moq;
using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Application.Services;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;
using Together.Domain.ValueObjects;
using Xunit;

namespace Together.Application.Tests.Services;

public class ProfileServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<IFollowService> _mockFollowService;
    private readonly ProfileService _profileService;

    public ProfileServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockStorageService = new Mock<IStorageService>();
        _mockFollowService = new Mock<IFollowService>();
        _profileService = new ProfileService(
            _mockUserRepository.Object, 
            _mockStorageService.Object,
            _mockFollowService.Object);
    }

    [Fact]
    public async Task GetProfileAsync_WithValidUserId_ReturnsProfileDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", Email.Create("test@example.com"), "hashedpassword");
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockFollowService.Setup(f => f.GetFollowerCountAsync(userId))
            .ReturnsAsync(10);
        _mockFollowService.Setup(f => f.GetFollowingCountAsync(userId))
            .ReturnsAsync(5);

        // Act
        var result = await _profileService.GetProfileAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal(10, result.FollowerCount);
        Assert.Equal(5, result.FollowingCount);
    }

    [Fact]
    public async Task GetProfileAsync_WithInvalidUserId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _profileService.GetProfileAsync(userId));
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_ReturnsUpdatedProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", Email.Create("test@example.com"), "hashedpassword");
        var updateDto = new UpdateProfileDto("New bio", null, ProfileVisibility.Public);
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockFollowService.Setup(f => f.GetFollowerCountAsync(userId))
            .ReturnsAsync(0);
        _mockFollowService.Setup(f => f.GetFollowingCountAsync(userId))
            .ReturnsAsync(0);

        // Act
        var result = await _profileService.UpdateProfileAsync(userId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New bio", result.Bio);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithLongBio_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", Email.Create("test@example.com"), "hashedpassword");
        var longBio = new string('a', 501); // 501 characters
        var updateDto = new UpdateProfileDto(longBio, null, ProfileVisibility.Public);
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _profileService.UpdateProfileAsync(userId, updateDto));
        
        Assert.Contains("Bio", exception.Errors.Keys);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WithValidImage_ReturnsUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", Email.Create("test@example.com"), "hashedpassword");
        var imageData = new byte[1024]; // 1KB image
        var fileName = "profile.jpg";
        var expectedUrl = "https://example.com/profile.jpg";
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockStorageService.Setup(s => s.UploadProfilePictureAsync(imageData, fileName, userId))
            .ReturnsAsync(expectedUrl);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _profileService.UploadProfilePictureAsync(userId, imageData, fileName);

        // Assert
        Assert.Equal(expectedUrl, result);
        _mockStorageService.Verify(s => s.UploadProfilePictureAsync(imageData, fileName, userId), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WithLargeFile_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", Email.Create("test@example.com"), "hashedpassword");
        var imageData = new byte[3 * 1024 * 1024]; // 3MB image (exceeds 2MB limit)
        var fileName = "profile.jpg";
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _profileService.UploadProfilePictureAsync(userId, imageData, fileName));
        
        Assert.Contains("ProfilePicture", exception.Errors.Keys);
    }

    [Fact]
    public async Task UploadProfilePictureAsync_WithInvalidExtension_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", Email.Create("test@example.com"), "hashedpassword");
        var imageData = new byte[1024]; // 1KB
        var fileName = "profile.gif"; // Invalid extension
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _profileService.UploadProfilePictureAsync(userId, imageData, fileName));
        
        Assert.Contains("ProfilePicture", exception.Errors.Keys);
    }
}
