using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Domain.Enums;

namespace Together.Presentation.ViewModels;

public class VirtualPetViewModel : ViewModelBase
{
    private readonly IVirtualPetService _petService;
    private readonly Guid _connectionId;

    private VirtualPetDto? _pet;
    private bool _isLoading;
    private string? _errorMessage;

    public VirtualPetViewModel(IVirtualPetService petService, Guid connectionId)
    {
        _petService = petService;
        _connectionId = connectionId;

        OpenCustomizationCommand = new RelayCommand(_ => OpenCustomization(), _ => Pet != null);
        RefreshCommand = new RelayCommand(async _ => await LoadPetAsync());

        _ = LoadPetAsync();
    }

    public VirtualPetDto? Pet
    {
        get => _pet;
        set => SetProperty(ref _pet, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string PetName => Pet?.Name ?? "Your Pet";
    public int Level => Pet?.Level ?? 1;
    public int ExperiencePoints => Pet?.ExperiencePoints ?? 0;
    public int ExperienceToNextLevel => Pet?.ExperienceToNextLevel ?? 100;
    public double ExperiencePercentage => Pet != null ? (double)Pet.ExperiencePoints / (Pet.ExperiencePoints + Pet.ExperienceToNextLevel) * 100 : 0;
    public PetState State => Pet?.State ?? PetState.Happy;
    public string StateDisplay => State.ToString();
    public string? AppearanceOptions => Pet?.AppearanceOptions ?? "default";

    public ICommand OpenCustomizationCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadPetAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Pet = await _petService.GetPetAsync(_connectionId);

            if (Pet == null)
            {
                ErrorMessage = "No virtual pet found for this connection";
            }

            OnPropertyChanged(nameof(PetName));
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(ExperiencePoints));
            OnPropertyChanged(nameof(ExperienceToNextLevel));
            OnPropertyChanged(nameof(ExperiencePercentage));
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(StateDisplay));
            OnPropertyChanged(nameof(AppearanceOptions));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load pet: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenCustomization()
    {
        // This will be handled by navigation service
        // For now, just a placeholder
    }

    public async Task UpdatePetStateAsync()
    {
        try
        {
            await _petService.UpdatePetStateAsync(_connectionId);
            await LoadPetAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to update pet state: {ex.Message}";
        }
    }
}
