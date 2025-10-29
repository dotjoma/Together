using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class VirtualPetService : IVirtualPetService
{
    private readonly IVirtualPetRepository _petRepository;
    private readonly ICoupleConnectionRepository _connectionRepository;

    // XP values for different interaction types
    private static readonly Dictionary<InteractionType, int> InteractionXpValues = new()
    {
        { InteractionType.JournalEntry, 20 },
        { InteractionType.MoodLog, 10 },
        { InteractionType.ChatMessage, 5 },
        { InteractionType.ChallengeCompletion, 30 },
        { InteractionType.TodoCompletion, 15 }
    };

    // Appearance unlocks by level
    private static readonly Dictionary<int, List<string>> LevelUnlocks = new()
    {
        { 1, new List<string> { "default", "blue", "pink" } },
        { 5, new List<string> { "green", "yellow" } },
        { 10, new List<string> { "purple", "orange" } },
        { 15, new List<string> { "rainbow", "galaxy" } },
        { 20, new List<string> { "golden", "diamond" } }
    };

    public VirtualPetService(
        IVirtualPetRepository petRepository,
        ICoupleConnectionRepository connectionRepository)
    {
        _petRepository = petRepository;
        _connectionRepository = connectionRepository;
    }

    public async Task<VirtualPetDto?> GetPetAsync(Guid connectionId)
    {
        var pet = await _petRepository.GetByConnectionIdAsync(connectionId);
        return pet != null ? MapToDto(pet) : null;
    }

    public async Task<VirtualPetDto> CreatePetAsync(Guid connectionId, string name)
    {
        // Verify connection exists
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        // Check if pet already exists
        var existingPet = await _petRepository.GetByConnectionIdAsync(connectionId);
        if (existingPet != null)
            throw new BusinessRuleViolationException("A virtual pet already exists for this connection");

        var pet = new VirtualPet(connectionId, name);
        await _petRepository.AddAsync(pet);

        return MapToDto(pet);
    }

    public async Task AddExperienceAsync(Guid connectionId, InteractionType interactionType)
    {
        var pet = await _petRepository.GetByConnectionIdAsync(connectionId);
        if (pet == null)
            throw new NotFoundException(nameof(VirtualPet), connectionId);

        // Get XP value for interaction type
        int xpToAdd = InteractionXpValues.GetValueOrDefault(interactionType, 5);

        // Add experience and check for level up
        pet.AddExperience(xpToAdd);

        // Update pet state to excited if leveled up
        if (pet.ExperiencePoints < 50) // Just leveled up (low XP in current level)
        {
            pet.UpdateState(PetState.Excited);
        }

        await _petRepository.UpdateAsync(pet);
    }

    public async Task UpdatePetStateAsync(Guid connectionId)
    {
        var pet = await _petRepository.GetByConnectionIdAsync(connectionId);
        if (pet == null)
            throw new NotFoundException(nameof(VirtualPet), connectionId);

        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        // Check last interaction date
        var lastInteraction = connection.LastInteractionDate ?? connection.EstablishedAt;
        var daysSinceLastInteraction = (DateTime.UtcNow - lastInteraction).TotalDays;

        PetState newState;
        if (daysSinceLastInteraction >= 3)
        {
            newState = PetState.Neglected;
        }
        else if (daysSinceLastInteraction >= 1)
        {
            newState = PetState.Sad;
        }
        else
        {
            newState = PetState.Happy;
        }

        if (pet.State != newState)
        {
            pet.UpdateState(newState);
            await _petRepository.UpdateAsync(pet);
        }
    }

    public async Task<VirtualPetDto> CustomizePetAsync(Guid petId, string? name, string? appearanceOptions)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
            throw new NotFoundException(nameof(VirtualPet), petId);

        if (!string.IsNullOrWhiteSpace(name))
        {
            pet.Rename(name);
        }

        if (!string.IsNullOrWhiteSpace(appearanceOptions))
        {
            // Validate that appearance is unlocked
            var unlockedAppearances = GetUnlockedAppearances(pet.Level);
            if (!unlockedAppearances.Contains(appearanceOptions))
            {
                throw new BusinessRuleViolationException($"Appearance '{appearanceOptions}' is not unlocked at level {pet.Level}");
            }

            pet.UpdateAppearance(appearanceOptions);
        }

        await _petRepository.UpdateAsync(pet);

        return MapToDto(pet);
    }

    private VirtualPetDto MapToDto(VirtualPet pet)
    {
        int experienceToNextLevel = pet.Level * 100 - pet.ExperiencePoints;
        var unlockedAppearances = GetUnlockedAppearances(pet.Level);

        return new VirtualPetDto(
            pet.Id,
            pet.ConnectionId,
            pet.Name ?? "Pet",
            pet.Level,
            pet.ExperiencePoints,
            experienceToNextLevel,
            pet.AppearanceOptions,
            pet.State,
            pet.CreatedAt,
            unlockedAppearances
        );
    }

    private List<string> GetUnlockedAppearances(int level)
    {
        var unlocked = new List<string>();

        foreach (var kvp in LevelUnlocks.OrderBy(x => x.Key))
        {
            if (level >= kvp.Key)
            {
                unlocked.AddRange(kvp.Value);
            }
        }

        return unlocked;
    }
}
