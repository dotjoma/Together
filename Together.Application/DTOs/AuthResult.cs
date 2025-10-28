namespace Together.Application.DTOs;

public record AuthResult(
    bool Success, 
    string? Token, 
    string Message, 
    UserDto? User);
