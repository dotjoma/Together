using System.Collections.ObjectModel;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class PetCustomizationViewModel : ViewModelBase
{
    private readonly IVirtualPetService _petService;
    private readonly Guid _petId;

    private VirtualPetDto? _pet;
    private string _petName = string.Empty;
    private string? _selectedAppearance;
    private bool _isLoading;
    private string? _errorMessage;
    private string? _successMessage;

    public PetCustomizationViewModel(IVirtualPetService petService, Guid petId)
    {
        _petService = petService;
        _petId = petId;

        SaveCommand = new RelayCommand(async _ => await SaveCustomizationAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());

        UnlockedAppearances = new ObservableCollection<string>();

        _ = LoadPetAsync();
    }

    public VirtualPetDto? Pet
    {
        get => _pet;
        set
        {
            SetProperty(ref _pet, value);
            if (value != null)
            {
                PetName = value.Name;
                SelectedAppearance = value.AppearanceOptions ?? "default";
                
                UnlockedAppearances.Clear();
                foreach (var appearance in value.UnlockedAppearances)
                {
                    UnlockedAppearances.Add(appearance);
                }
            }
        }
    }

    public string PetName
    {
        get => _petName;
        set => SetProperty(ref _petName, value);
    }

    public string? SelectedAppearance
    {
        get => _selectedAppearance;
        set => SetProperty(ref _selectedAppearance, value);
    }

    public ObservableCollection<string> UnlockedAppearances { get; }

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

    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private Task LoadPetAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // We need to get the pet by connection ID first
            // For now, we'll assume the pet is loaded from parent view
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load pet: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }

        return Task.CompletedTask;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(PetName) && !IsLoading;
    }

    private async Task SaveCustomizationAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var updatedPet = await _petService.CustomizePetAsync(_petId, PetName, SelectedAppearance);
            Pet = updatedPet;

            SuccessMessage = "Pet customization saved successfully!";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save customization: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Cancel()
    {
        // Reset to original values
        if (Pet != null)
        {
            PetName = Pet.Name;
            SelectedAppearance = Pet.AppearanceOptions ?? "default";
        }
    }
}
