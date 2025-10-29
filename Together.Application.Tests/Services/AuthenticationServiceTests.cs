using Microsoft.Extensions.Logging;
using Moq;
using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Services;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Domain.ValueObjects;
using Xunit;

namespace Together.Application.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<AuthenticationService>>();
        _authenticationService = new AuthenticationService(
            _mockUserRepository.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "Password123");
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authenticationService.RegisterAsync(registerDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("testuser", result.User.Username);
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_ThrowsValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "Pass1");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.RegisterAsync(registerDto));
        
        Assert.Contains("Password", exception.Errors.Keys);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMissingUppercase_ThrowsValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "password123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.RegisterAsync(registerDto));
        
        Assert.Contains("Password", exception.Errors.Keys);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMissingLowercase_ThrowsValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "PASSWORD123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.RegisterAsync(registerDto));
        
        Assert.Contains("Password", exception.Errors.Keys);
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMissingNumber_ThrowsValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "Password");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.RegisterAsync(registerDto));
        
        Assert.Contains("Password", exception.Errors.Keys);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "Password123");
        var existingUser = User.Create("existinguser", Email.Create("test@example.com"), "hashedpassword");
        
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.RegisterAsync(registerDto));
        
        Assert.Contains("Email", exception.Errors.Keys);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ThrowsValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "Password123");
        var existingUser = User.Create("testuser", Email.Create("other@example.com"), "hashedpassword");
        
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _authenticationService.RegisterAsync(registerDto));
        
        Assert.Contains("Username", exception.Errors.Keys);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var password = "Password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = User.Create("testuser", Email.Create("test@example.com"), passwordHash);
        
        var loginDto = new LoginDto("test@example.com", password);
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authenticationService.LoginAsync(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("testuser", result.User.Username);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ReturnsFailureResult()
    {
        // Arrange
        var loginDto = new LoginDto("nonexistent@example.com", "Password123");
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authenticationService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var correctPassword = "Password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        var user = User.Create("testuser", Email.Create("test@example.com"), passwordHash);
        
        var loginDto = new LoginDto("test@example.com", "WrongPassword123");
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authenticationService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var registerDto = new RegisterDto("testuser", "test@example.com", "Password123");
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var registerResult = await _authenticationService.RegisterAsync(registerDto);
        var token = registerResult.Token!;

        // Act
        var isValid = await _authenticationService.ValidateTokenAsync(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = await _authenticationService.ValidateTokenAsync(invalidToken);

        // Assert
        Assert.False(isValid);
    }
}
