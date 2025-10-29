using System;
using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class LongDistanceViewModel : ViewModelBase
{
    private readonly ILongDistanceService _longDistanceService;
    private readonly Guid _connectionId;
    private readonly Guid _userId;

    private DistanceWidgetViewModel? _distanceWidgetViewModel;

    public LongDistanceViewModel(
        ILongDistanceService longDistanceService, 
        Guid connectionId,
        Guid userId)
    {
        _longDistanceService = longDistanceService;
        _connectionId = connectionId;
        _userId = userId;

        DistanceWidgetViewModel = new DistanceWidgetViewModel(longDistanceService, connectionId);
        OpenLocationSettingsCommand = new RelayCommand(_ => OpenLocationSettings());
    }

    public DistanceWidgetViewModel? DistanceWidgetViewModel
    {
        get => _distanceWidgetViewModel;
        set => SetProperty(ref _distanceWidgetViewModel, value);
    }

    public ICommand OpenLocationSettingsCommand { get; }

    private void OpenLocationSettings()
    {
        // In a real implementation, this would open a dialog or navigate to location settings
        // For now, we'll just create the view model
        var locationSettingsViewModel = new LocationSettingsViewModel(_longDistanceService, _userId);
        
        // You would typically show this in a dialog or navigate to it
        // Example: NavigationService.NavigateTo(locationSettingsViewModel);
    }
}
