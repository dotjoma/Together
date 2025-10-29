using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class LocationSettingsViewModel : ViewModelBase
{
    private readonly ILongDistanceService _longDistanceService;
    private readonly Guid _userId;

    private string _latitude = string.Empty;
    private string _longitude = string.Empty;
    private TimeZoneInfo? _selectedTimeZone;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

    public LocationSettingsViewModel(ILongDistanceService longDistanceService, Guid userId)
    {
        _longDistanceService = longDistanceService;
        _userId = userId;

        // Load all available time zones
        AvailableTimeZones = new ObservableCollection<TimeZoneInfo>(
            TimeZoneInfo.GetSystemTimeZones().OrderBy(tz => tz.BaseUtcOffset));

        // Set default to local timezone
        SelectedTimeZone = TimeZoneInfo.Local;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
        ClearLocationCommand = new RelayCommand(async _ => await ClearLocationAsync());
    }

    public string Latitude
    {
        get => _latitude;
        set
        {
            SetProperty(ref _latitude, value);
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public string Longitude
    {
        get => _longitude;
        set
        {
            SetProperty(ref _longitude, value);
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public TimeZoneInfo? SelectedTimeZone
    {
        get => _selectedTimeZone;
        set
        {
            SetProperty(ref _selectedTimeZone, value);
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public ObservableCollection<TimeZoneInfo> AvailableTimeZones { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public bool CanSave => 
        !string.IsNullOrWhiteSpace(Latitude) && 
        !string.IsNullOrWhiteSpace(Longitude) && 
        SelectedTimeZone != null && 
        !IsLoading;

    public ICommand SaveCommand { get; }
    public ICommand ClearLocationCommand { get; }

    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Parse latitude and longitude
            if (!double.TryParse(Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
            {
                ErrorMessage = "Invalid latitude format. Use decimal format (e.g., 40.7128)";
                return;
            }

            if (!double.TryParse(Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
            {
                ErrorMessage = "Invalid longitude format. Use decimal format (e.g., -74.0060)";
                return;
            }

            if (SelectedTimeZone == null)
            {
                ErrorMessage = "Please select a time zone";
                return;
            }

            await _longDistanceService.UpdateUserLocationAsync(_userId, lat, lon, SelectedTimeZone.Id);
            SuccessMessage = "Location saved successfully!";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving location: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task ClearLocationAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            Latitude = string.Empty;
            Longitude = string.Empty;
            SelectedTimeZone = TimeZoneInfo.Local;

            // In a real implementation, you'd call a method to clear the location
            // For now, we'll just clear the fields
            SuccessMessage = "Location cleared";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing location: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }

        return Task.CompletedTask;
    }
}
