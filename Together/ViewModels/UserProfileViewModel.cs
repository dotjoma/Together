using System.IO;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Domain.Enums;

namespace Together.Presentation.ViewModels;

public class UserProfileViewModel : ViewModelBase
{
    private readonly IProfileService _profileService;
    private readonly Guid _userId;

    private ProfileDto? _profile;
    private bool _isEditMode;
    private string? _editBio;
    private ProfileVisibility _editVisibility;
    private bool _isLoading;
    private string? _errorMessage;
    private byte[]? _selectedImageData;
    private string? _selectedImageFileName;

    public UserProfileViewModel(IProfileService profileService, Guid userId)
    {
        _profileService = profileService;
        _userId = userId;

        EditProfileCommand = new RelayCommand(async _ => await EnterEditModeAsync(), _ => !IsEditMode && !IsLoading);
        SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync(), _ => IsEditMode && !IsLoading);
        CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditMode && !IsLoading);
        SelectImageCommand = new RelayCommand(async _ => await SelectImageAsync(), _ => IsEditMode && !IsLoading);

        _ = LoadProfileAsync();
    }

    public ProfileDto? Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string? EditBio
    {
        get => _editBio;
        set => SetProperty(ref _editBio, value);
    }

    public ProfileVisibility EditVisibility
    {
        get => _editVisibility;
        set => SetProperty(ref _editVisibility, value);
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

    public string? SelectedImageFileName
    {
        get => _selectedImageFileName;
        set => SetProperty(ref _selectedImageFileName, value);
    }

    public ICommand EditProfileCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SelectImageCommand { get; }

    private async Task LoadProfileAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            Profile = await _profileService.GetProfileAsync(_userId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task EnterEditModeAsync()
    {
        if (Profile == null) return Task.CompletedTask;

        IsEditMode = true;
        EditBio = Profile.Bio;
        EditVisibility = Profile.Visibility;
        _selectedImageData = null;
        SelectedImageFileName = null;
        ErrorMessage = null;

        return Task.CompletedTask;
    }

    private async Task SaveProfileAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Upload profile picture if selected
            string? newProfilePictureUrl = Profile?.ProfilePictureUrl;
            if (_selectedImageData != null && !string.IsNullOrWhiteSpace(_selectedImageFileName))
            {
                newProfilePictureUrl = await _profileService.UploadProfilePictureAsync(
                    _userId, 
                    _selectedImageData, 
                    _selectedImageFileName);
            }

            // Update profile
            var updateDto = new UpdateProfileDto(EditBio, newProfilePictureUrl, EditVisibility);
            Profile = await _profileService.UpdateProfileAsync(_userId, updateDto);

            IsEditMode = false;
            _selectedImageData = null;
            SelectedImageFileName = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        EditBio = Profile?.Bio;
        EditVisibility = Profile?.Visibility ?? ProfileVisibility.Public;
        _selectedImageData = null;
        SelectedImageFileName = null;
        ErrorMessage = null;
    }

    private async Task SelectImageAsync()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Select Profile Picture"
            };

            if (dialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(dialog.FileName);
                
                // Check file size (2MB max)
                const long maxFileSize = 2 * 1024 * 1024;
                if (fileInfo.Length > maxFileSize)
                {
                    ErrorMessage = "Profile picture must be less than 2MB";
                    return;
                }

                _selectedImageData = await File.ReadAllBytesAsync(dialog.FileName);
                SelectedImageFileName = Path.GetFileName(dialog.FileName);
                ErrorMessage = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to select image: {ex.Message}";
        }
    }
}
