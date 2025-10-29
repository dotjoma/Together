using Together.Application.DTOs;
using Together.Domain.Enums;

namespace Together.Application.Interfaces;

public interface IVirtualPetService
{
    Task<VirtualPetDto?> GetPetAsync(Guid connectionId);
    Task<VirtualPetDto> CreatePetAsync(Guid connectionId, string name);
    Task AddExperienceAsync(Guid connectionId, InteractionType interactionType);
    Task UpdatePetStateAsync(Guid connectionId);
    Task<VirtualPetDto> CustomizePetAsync(Guid petId, string? name, string? appearanceOptions);
}
