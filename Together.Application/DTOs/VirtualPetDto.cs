using Together.Domain.Enums;

namespace Together.Application.DTOs;

public record VirtualPetDto(
    Guid Id,
    Guid ConnectionId,
    string Name,
    int Level,
    int ExperiencePoints,
    int ExperienceToNextLevel,
    string? AppearanceOptions,
    PetState State,
    DateTime CreatedAt,
    List<string> UnlockedAppearances
);
