using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
    private const int BcryptWorkFactor = 12;
    private const string DefaultJwtSecret = "ThisIsATestSecretKeyForDevelopmentOnlyPleaseChangeInProduction123456";

    public AuthenticationService(IUserRepository userRepository, IConfiguration? configuration = null)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        // Validate input
        var validationErrors = ValidateRegistration(dto);
        if (validationErrors.Count > 0)
        {
            throw new ValidationException(validationErrors);
        }

        // Check if user already exists
        var existingUserByEmail = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUserByEmail != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", new[] { "A user with this email already exists" } }
            });
        }

        var existingUserByUsername = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingUserByUsername != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Username", new[] { "A user with this username already exists" } }
            });
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BcryptWorkFactor);

        // Create user
        var email = Email.Create(dto.Email);
        var user = User.Create(dto.Username, email, passwordHash);

        await _userRepository.AddAsync(user);

        // Generate token
        var token = GenerateJwtToken(user);

        var userDto = MapToUserDto(user);
        return AuthResultExtensions.Success(token, "Registration successful", userDto);
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return AuthResultExtensions.Failure("Invalid email or password");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return AuthResultExtensions.Failure("Invalid email or password");
        }

        // Generate token
        var token = GenerateJwtToken(user);

        var userDto = MapToUserDto(user);
        return AuthResultExtensions.Success(token, "Login successful", userDto);
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
