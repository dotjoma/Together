using Together.Domain.Enums;

namespace Together.Application.DTOs;

public record ProfileDto(
    Guid Id,
    string Username,
    string Email,
    string? ProfilePictureUrl,
    string? Bio,
    ProfileVisibility Visibility,
    int FollowerCount,
    int FollowingCount,
    DateTime CreatedAt);
