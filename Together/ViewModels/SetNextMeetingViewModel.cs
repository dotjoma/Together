using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class SetNextMeetingViewModel : ViewModelBase
{
    private readonly ILongDistanceService _longDistanceService;
    private readonly Guid _connectionId;
    private readonly Action _onSaved;

    private DateTime? _nextMeetingDate;
    private DateTime? _nextMeetingTime;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public SetNextMeetingViewModel(
        ILongDistanceService longDistanceService, 
        Guid connectionId,
        Action onSaved)
    {
        _longDistanceService = longDistanceService;
        _connectionId = connectionId;
        _onSaved = onSaved;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
        CancelCommand = new RelayCommand(_ => _onSaved?.Invoke());

        // Set default to tomorrow
        NextMeetingDate = DateTime.Today.AddDays(1);
        NextMeetingTime = DateTime.Today.AddHours(12); // Noon
    }

    public DateTime? NextMeetingDate
    {
        get => _nextMeetingDate;
        set
        {
            SetProperty(ref _nextMeetingDate, value);
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public DateTime? NextMeetingTime
    {
        get => _nextMeetingTime;
        set
        {
            SetProperty(ref _nextMeetingTime, value);
            OnPropertyChanged(nameof(CanSave));
        }
    }

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

    public bool CanSave => NextMeetingDate.HasValue && NextMeetingTime.HasValue && !IsLoading;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (!NextMeetingDate.HasValue || !NextMeetingTime.HasValue)
            {
                ErrorMessage = "Please select both date and time";
                return;
            }

            // Combine date and time
            var meetingDateTime = NextMeetingDate.Value.Date
                .AddHours(NextMeetingTime.Value.Hour)
                .AddMinutes(NextMeetingTime.Value.Minute);

            // Convert to UTC
            var meetingDateTimeUtc = meetingDateTime.ToUniversalTime();

            if (meetingDateTimeUtc <= DateTime.UtcNow)
            {
                ErrorMessage = "Meeting date must be in the future";
                return;
            }

            await _longDistanceService.SetNextMeetingDateAsync(_connectionId, meetingDateTimeUtc);
            _onSaved?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving meeting date: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
