using Together.Domain.Enums;

namespace Together.Application.DTOs;

public record UpdateProfileDto(
    string? Bio,
    string? ProfilePictureUrl,
    ProfileVisibility Visibility);
