using System;
using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Services;

namespace Together.Presentation.ViewModels;

public class LongDistanceViewModel : ViewModelBase, INavigationAware
{
    private readonly ILongDistanceService _longDistanceService;
    private readonly ICoupleConnectionService _coupleConnectionService;
    private Guid _connectionId;
    private Guid _userId;

    private DistanceWidgetViewModel? _distanceWidgetViewModel;

    public LongDistanceViewModel(
        ILongDistanceService longDistanceService,
        ICoupleConnectionService coupleConnectionService)
    {
        _longDistanceService = longDistanceService;
        _coupleConnectionService = coupleConnectionService;

        OpenLocationSettingsCommand = new RelayCommand(_ => OpenLocationSettings());
    }

    public DistanceWidgetViewModel? DistanceWidgetViewModel
    {
        get => _distanceWidgetViewModel;
        set => SetProperty(ref _distanceWidgetViewModel, value);
    }

    public ICommand OpenLocationSettingsCommand { get; }

    public async void OnNavigatedTo(object? parameter)
    {
        // Get current user from application properties
        var currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as Application.DTOs.UserDto;
        if (currentUser != null)
        {
            _userId = currentUser.Id;
            
            // Get couple connection
            var connection = await _coupleConnectionService.GetUserConnectionAsync(_userId);
            if (connection != null)
            {
                _connectionId = connection.Id;
                DistanceWidgetViewModel = new DistanceWidgetViewModel(_longDistanceService, _connectionId);
            }
        }
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if needed
    }

    private void OpenLocationSettings()
    {
        // In a real implementation, this would open a dialog or navigate to location settings
        // For now, we'll just create the view model
        var locationSettingsViewModel = new LocationSettingsViewModel(_longDistanceService, _userId);
        
        // You would typically show this in a dialog or navigate to it
        // Example: NavigationService.NavigateTo(locationSettingsViewModel);
    }
}
