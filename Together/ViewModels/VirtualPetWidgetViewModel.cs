using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Domain.Enums;

namespace Together.Presentation.ViewModels;

public class VirtualPetWidgetViewModel : ViewModelBase
{
    private readonly IVirtualPetService _petService;
    private readonly Guid _connectionId;

    private VirtualPetDto? _pet;

    public VirtualPetWidgetViewModel(IVirtualPetService petService, Guid connectionId)
    {
        _petService = petService;
        _connectionId = connectionId;

        _ = LoadPetAsync();
    }

    public VirtualPetDto? Pet
    {
        get => _pet;
        set
        {
            SetProperty(ref _pet, value);
            OnPropertyChanged(nameof(PetName));
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(ExperiencePoints));
            OnPropertyChanged(nameof(ExperienceToNextLevel));
            OnPropertyChanged(nameof(ExperiencePercentage));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(StateDisplay));
            OnPropertyChanged(nameof(AppearanceOptions));
        }
    }

    public string PetName => Pet?.Name ?? "Your Pet";
    public int Level => Pet?.Level ?? 1;
    public int ExperiencePoints => Pet?.ExperiencePoints ?? 0;
    public int ExperienceToNextLevel => Pet?.ExperienceToNextLevel ?? 100;
    public double ExperiencePercentage => Pet != null ? (double)Pet.ExperiencePoints / (Pet.ExperiencePoints + Pet.ExperienceToNextLevel) * 100 : 0;
    public PetState State => Pet?.State ?? PetState.Happy;
    public string StateDisplay => State.ToString();
    public string? AppearanceOptions => Pet?.AppearanceOptions ?? "default";

    private async Task LoadPetAsync()
    {
        try
        {
            Pet = await _petService.GetPetAsync(_connectionId);
        }
        catch (Exception)
        {
            // Silently fail for widget
        }
    }

    public async Task RefreshAsync()
    {
        await LoadPetAsync();
    }
}
