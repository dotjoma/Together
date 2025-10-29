using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class DistanceWidgetViewModel : ViewModelBase
{
    private readonly ILongDistanceService _longDistanceService;
    private readonly Guid _connectionId;
    private readonly DispatcherTimer _timer;

    private bool _isLoading;
    private bool _hasDistance;
    private bool _hasNextMeeting;
    private bool _hasTimeZones;
    private bool _hasOptimalWindow;
    private string _distanceDisplay = "0";
    private string _distanceUnit = "km";
    private int _daysUntilMeeting;
    private int _hoursUntilMeeting;
    private int _minutesUntilMeeting;
    private int _secondsUntilMeeting;
    private string _nextMeetingDateDisplay = string.Empty;
    private DateTime? _user1LocalTime;
    private DateTime? _user2LocalTime;
    private string _user1TimeZone = string.Empty;
    private string _user2TimeZone = string.Empty;
    private string _optimalWindowDisplay = string.Empty;

    public DistanceWidgetViewModel(ILongDistanceService longDistanceService, Guid connectionId)
    {
        _longDistanceService = longDistanceService;
        _connectionId = connectionId;

        SetNextMeetingCommand = new RelayCommand(async _ => await SetNextMeetingAsync());

        // Update timer every second for countdown
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += async (s, e) => await LoadDataAsync();
        _timer.Start();

        _ = LoadDataAsync();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasDistance
    {
        get => _hasDistance;
        set => SetProperty(ref _hasDistance, value);
    }

    public bool HasNextMeeting
    {
        get => _hasNextMeeting;
        set => SetProperty(ref _hasNextMeeting, value);
    }

    public bool HasTimeZones
    {
        get => _hasTimeZones;
        set => SetProperty(ref _hasTimeZones, value);
    }

    public bool HasOptimalWindow
    {
        get => _hasOptimalWindow;
        set => SetProperty(ref _hasOptimalWindow, value);
    }

    public string DistanceDisplay
    {
        get => _distanceDisplay;
        set => SetProperty(ref _distanceDisplay, value);
    }

    public string DistanceUnit
    {
        get => _distanceUnit;
        set => SetProperty(ref _distanceUnit, value);
    }

    public int DaysUntilMeeting
    {
        get => _daysUntilMeeting;
        set => SetProperty(ref _daysUntilMeeting, value);
    }

    public int HoursUntilMeeting
    {
        get => _hoursUntilMeeting;
        set => SetProperty(ref _hoursUntilMeeting, value);
    }

    public int MinutesUntilMeeting
    {
        get => _minutesUntilMeeting;
        set => SetProperty(ref _minutesUntilMeeting, value);
    }

    public int SecondsUntilMeeting
    {
        get => _secondsUntilMeeting;
        set => SetProperty(ref _secondsUntilMeeting, value);
    }

    public string NextMeetingDateDisplay
    {
        get => _nextMeetingDateDisplay;
        set => SetProperty(ref _nextMeetingDateDisplay, value);
    }

    public DateTime? User1LocalTime
    {
        get => _user1LocalTime;
        set => SetProperty(ref _user1LocalTime, value);
    }

    public DateTime? User2LocalTime
    {
        get => _user2LocalTime;
        set => SetProperty(ref _user2LocalTime, value);
    }

    public string User1TimeZone
    {
        get => _user1TimeZone;
        set => SetProperty(ref _user1TimeZone, value);
    }

    public string User2TimeZone
    {
        get => _user2TimeZone;
        set => SetProperty(ref _user2TimeZone, value);
    }

    public string OptimalWindowDisplay
    {
        get => _optimalWindowDisplay;
        set => SetProperty(ref _optimalWindowDisplay, value);
    }

    public ICommand SetNextMeetingCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            var info = await _longDistanceService.GetLongDistanceInfoAsync(_connectionId);

            // Update distance
            HasDistance = info.DistanceInKilometers.HasValue;
            if (HasDistance)
            {
                // Use miles for US, km for others (simplified)
                DistanceDisplay = info.DistanceInMiles.HasValue 
                    ? $"{info.DistanceInMiles.Value:F0}" 
                    : $"{info.DistanceInKilometers!.Value:F0}";
                DistanceUnit = info.DistanceInMiles.HasValue ? "miles" : "km";
            }

            // Update countdown
            HasNextMeeting = info.NextMeetingDate.HasValue && info.TimeUntilMeeting.HasValue;
            if (HasNextMeeting)
            {
                var timeSpan = info.TimeUntilMeeting!.Value;
                DaysUntilMeeting = timeSpan.Days;
                HoursUntilMeeting = timeSpan.Hours;
                MinutesUntilMeeting = timeSpan.Minutes;
                SecondsUntilMeeting = timeSpan.Seconds;
                NextMeetingDateDisplay = $"Meeting on {info.NextMeetingDate!.Value:MMMM dd, yyyy}";
            }

            // Update local times
            HasTimeZones = info.User1LocalTime.HasValue && info.User2LocalTime.HasValue;
            if (HasTimeZones)
            {
                User1LocalTime = info.User1LocalTime;
                User2LocalTime = info.User2LocalTime;
                User1TimeZone = GetShortTimeZoneName(info.User1TimeZone);
                User2TimeZone = GetShortTimeZoneName(info.User2TimeZone);
            }

            // Update optimal window
            HasOptimalWindow = info.OptimalWindow != null;
            if (HasOptimalWindow)
            {
                var window = info.OptimalWindow!;
                OptimalWindowDisplay = $"{window.OverlapStart:HH:mm} - {window.OverlapEnd:HH:mm} (your time)";
            }
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error loading long distance info: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SetNextMeetingAsync()
    {
        // This would open a dialog to set the next meeting date
        // For now, we'll just show a placeholder
        // In a real implementation, you'd use a DatePicker dialog
        var nextMeeting = DateTime.UtcNow.AddDays(30); // Example: 30 days from now
        await _longDistanceService.SetNextMeetingDateAsync(_connectionId, nextMeeting);
        await LoadDataAsync();
    }

    private string GetShortTimeZoneName(string? timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
            return string.Empty;

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return tz.DisplayName.Split(' ')[0]; // Get first part of display name
        }
        catch
        {
            return timeZoneId;
        }
    }

    public void Dispose()
    {
        _timer?.Stop();
    }
}
