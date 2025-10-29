using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Together.Application.Common;
using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;
using Together.Domain.ValueObjects;

namespace Together.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration? _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IInputValidator? _inputValidator;
    private readonly IAuditLogger? _auditLogger;
    private const int BcryptWorkFactor = 12;
    private const string DefaultJwtSecret = "ThisIsATestSecretKeyForDevelopmentOnlyPleaseChangeInProduction123456";

    public AuthenticationService(
        IUserRepository userRepository, 
        ILogger<AuthenticationService> logger,
        IConfiguration? configuration = null,
        IInputValidator? inputValidator = null,
        IAuditLogger? auditLogger = null)
    {
        _userRepository = userRepository;
        _logger = logger;
        _configuration = configuration;
        _inputValidator = inputValidator;
        _auditLogger = auditLogger;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        using var scope = _logger.BeginCorrelationScope("UserRegistration");
        _logger.LogInformationWithCorrelation("Starting user registration for username: {Username}", dto.Username);

        try
        {
            // Sanitize inputs
            var sanitizedUsername = _inputValidator?.SanitizeText(dto.Username) ?? dto.Username;
            var sanitizedEmail = _inputValidator?.SanitizeText(dto.Email) ?? dto.Email;

            // Check for injection patterns
            if (_inputValidator != null)
            {
                if (_inputValidator.ContainsSqlInjectionPatterns(dto.Username) ||
                    _inputValidator.ContainsSqlInjectionPatterns(dto.Email))
                {
                    if (_auditLogger != null)
                    {
                        await _auditLogger.LogSecurityViolationAsync(null, "SQLInjectionAttempt", 
                            $"Registration attempt with SQL injection patterns: {dto.Username}");
                    }
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "Security", new[] { "Invalid input detected" } }
                    });
                }

                if (_inputValidator.ContainsXssPatterns(dto.Username) ||
                    _inputValidator.ContainsXssPatterns(dto.Email))
                {
                    if (_auditLogger != null)
                    {
                        await _auditLogger.LogSecurityViolationAsync(null, "XSSAttempt", 
                            $"Registration attempt with XSS patterns: {dto.Username}");
                    }
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "Security", new[] { "Invalid input detected" } }
                    });
                }
            }

            // Validate input
            var validationErrors = ValidateRegistration(new RegisterDto(sanitizedUsername, sanitizedEmail, dto.Password));
            if (validationErrors.Count > 0)
            {
                _logger.LogWarningWithCorrelation("Registration validation failed for username: {Username}", sanitizedUsername);
                if (_auditLogger != null)
                {
                    await _auditLogger.LogAuthenticationEventAsync(null, "Registration", false, "Validation failed");
                }
                throw new ValidationException(validationErrors);
            }

            // Check if user already exists
            var existingUserByEmail = await _userRepository.GetByEmailAsync(sanitizedEmail);
            if (existingUserByEmail != null)
            {
                _logger.LogWarningWithCorrelation("Registration failed: Email already exists for username: {Username}", sanitizedUsername);
                if (_auditLogger != null)
                {
                    await _auditLogger.LogAuthenticationEventAsync(null, "Registration", false, "Email already exists");
                }
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Email", new[] { "A user with this email already exists" } }
                });
            }

            var existingUserByUsername = await _userRepository.GetByUsernameAsync(sanitizedUsername);
            if (existingUserByUsername != null)
            {
                _logger.LogWarningWithCorrelation("Registration failed: Username already exists: {Username}", sanitizedUsername);
                if (_auditLogger != null)
                {
                    await _auditLogger.LogAuthenticationEventAsync(null, "Registration", false, "Username already exists");
                }
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Username", new[] { "A user with this username already exists" } }
                });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BcryptWorkFactor);

            // Create user
            var email = Email.Create(sanitizedEmail);
            var user = User.Create(sanitizedUsername, email, passwordHash);

            await _userRepository.AddAsync(user);
            _logger.LogInformationWithCorrelation("User registered successfully: {Username}, UserId: {UserId}", sanitizedUsername, user.Id);
            if (_auditLogger != null)
            {
                await _auditLogger.LogAuthenticationEventAsync(user.Id, "Registration", true, "User registered successfully");
            }

            // Generate token
            var token = GenerateJwtToken(user);

            var userDto = MapToUserDto(user);
            return AuthResultExtensions.Success(token, "Registration successful", userDto);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogErrorWithCorrelation(ex, "Error during user registration for username: {Username}", dto.Username);
            throw;
        }
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        using var scope = _logger.BeginCorrelationScope("UserLogin");
        _logger.LogInformationWithCorrelation("Login attempt for email: {Email}", dto.Email);

        try
        {
            // Sanitize email input
            var sanitizedEmail = _inputValidator?.SanitizeText(dto.Email) ?? dto.Email;

            // Check for injection patterns
            if (_inputValidator != null)
            {
                if (_inputValidator.ContainsSqlInjectionPatterns(sanitizedEmail))
                {
                    if (_auditLogger != null)
                    {
                        await _auditLogger.LogSecurityViolationAsync(null, "SQLInjectionAttempt", 
                            $"Login attempt with SQL injection patterns: {sanitizedEmail}");
                    }
                    return AuthResultExtensions.Failure("Invalid email or password");
                }
            }

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(sanitizedEmail);
            if (user == null)
            {
                _logger.LogWarningWithCorrelation("Login failed: User not found for email: {Email}", sanitizedEmail);
                if (_auditLogger != null)
                {
                    await _auditLogger.LogAuthenticationEventAsync(null, "Login", false, "User not found");
                }
                return AuthResultExtensions.Failure("Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarningWithCorrelation("Login failed: Invalid password for email: {Email}", sanitizedEmail);
                if (_auditLogger != null)
                {
                    await _auditLogger.LogAuthenticationEventAsync(user.Id, "Login", false, "Invalid password");
                }
                return AuthResultExtensions.Failure("Invalid email or password");
            }

            // Generate token
            var token = GenerateJwtToken(user);

            _logger.LogInformationWithCorrelation("User logged in successfully: {Username}, UserId: {UserId}", user.Username, user.Id);
            if (_auditLogger != null)
            {
                await _auditLogger.LogAuthenticationEventAsync(user.Id, "Login", true, "Login successful");
            }

            var userDto = MapToUserDto(user);
            return AuthResultExtensions.Success(token, "Login successful", userDto);
        }
        catch (Exception ex)
        {
            _logger.LogErrorWithCorrelation(ex, "Error during login for email: {Email}", dto.Email);
            throw;
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration?["Jwt:Issuer"] ?? "Together",
                ValidateAudience = true,
                ValidAudience = _configuration?["Jwt:Audience"] ?? "TogetherUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal if user exists
            return true;
        }

        // Generate password reset token (simplified - in production, store this in database)
        var resetToken = GeneratePasswordResetToken(user);

        // TODO: Send email with reset link
        // For now, just return true
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        // Validate new password
        var passwordErrors = ValidatePassword(newPassword);
        if (passwordErrors.Length > 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Password", passwordErrors }
            });
        }

        try
        {
            // Validate and decode token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration?["Jwt:Issuer"] ?? "Together",
                ValidateAudience = true,
                ValidAudience = _configuration?["Jwt:Audience"] ?? "TogetherUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return false;
            }

            // Get user and update password
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, BcryptWorkFactor);
            user.UpdatePassword(newPasswordHash);

            await _userRepository.UpdateAsync(user);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private Dictionary<string, string[]> ValidateRegistration(RegisterDto dto)
    {
        var errors = new Dictionary<string, string[]>();

        // Validate username
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            errors["Username"] = new[] { "Username is required" };
        }
        else if (dto.Username.Length < 3 || dto.Username.Length > 50)
        {
            errors["Username"] = new[] { "Username must be between 3 and 50 characters" };
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            errors["Email"] = new[] { "Email is required" };
        }
        else if (!Email.IsValid(dto.Email))
        {
            errors["Email"] = new[] { "Invalid email format" };
        }

        // Validate password
        var passwordErrors = ValidatePassword(dto.Password);
        if (passwordErrors.Length > 0)
        {
            errors["Password"] = passwordErrors;
        }

        return errors;
    }

    private string[] ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required");
            return errors.ToArray();
        }

        if (password.Length < 8)
        {
            errors.Add("Password must be at least 8 characters long");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            errors.Add("Password must contain at least one uppercase letter");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            errors.Add("Password must contain at least one lowercase letter");
        }

        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            errors.Add("Password must contain at least one number");
        }

        return errors.ToArray();
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtSecret());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email.Value)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _configuration?["Jwt:Issuer"] ?? "Together",
            Audience = _configuration?["Jwt:Audience"] ?? "TogetherUsers",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GeneratePasswordResetToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(GetJwtSecret());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("purpose", "password-reset")
            }),
            Expires = DateTime.UtcNow.AddHours(1), // Reset token expires in 1 hour
            Issuer = _configuration?["Jwt:Issuer"] ?? "Together",
            Audience = _configuration?["Jwt:Audience"] ?? "TogetherUsers",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GetJwtSecret()
    {
        var secret = _configuration?["Jwt:Secret"];
        if (string.IsNullOrEmpty(secret))
        {
            return DefaultJwtSecret;
        }
        return secret;
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.ProfilePictureUrl,
            user.Bio
        );
    }
}

public static class AuthResultExtensions
{
    public static AuthResult Success(string token, string message, UserDto user)
    {
        return new AuthResult(true, token, message, user);
    }

    public static AuthResult Failure(string message)
    {
        return new AuthResult(false, null, message, null);
    }
}
