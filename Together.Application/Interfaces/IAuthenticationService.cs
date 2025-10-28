using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
