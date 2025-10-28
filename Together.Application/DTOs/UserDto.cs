namespace Together.Application.DTOs;

public record UserDto(
    Guid Id, 
    string Username, 
    string Email, 
    string? ProfilePictureUrl, 
    string? Bio);
