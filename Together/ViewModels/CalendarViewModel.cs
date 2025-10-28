using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class CalendarViewModel : ViewModelBase
{
    private readonly IEventService _eventService;
    private readonly Guid _currentUserId;

    private DateTime _currentMonth;
    private int _daysTogether;
    private ObservableCollection<SharedEventDto> _upcomingEvents;
    private ObservableCollection<SharedEventDto> _monthEvents;
    private SharedEventDto? _selectedEvent;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public CalendarViewModel(IEventService eventService, Guid currentUserId)
    {
        _eventService = eventService;
        _currentUserId = currentUserId;
        _currentMonth = DateTime.Now;
        _upcomingEvents = new ObservableCollection<SharedEventDto>();
        _monthEvents = new ObservableCollection<SharedEventDto>();

        PreviousMonthCommand = new RelayCommand(_ => NavigateToPreviousMonth());
        NextMonthCommand = new RelayCommand(_ => NavigateToNextMonth());
        TodayCommand = new RelayCommand(_ => NavigateToToday());
        CreateEventCommand = new RelayCommand(_ => OnCreateEventRequested?.Invoke());
        EditEventCommand = new RelayCommand(_ => OnEditEventRequested?.Invoke(SelectedEvent!), _ => SelectedEvent != null);
        DeleteEventCommand = new RelayCommand(async _ => await DeleteEventAsync(), _ => SelectedEvent != null);
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
    }

    public DateTime CurrentMonth
    {
        get => _currentMonth;
        set
        {
            if (SetProperty(ref _currentMonth, value))
            {
                _ = LoadMonthEventsAsync();
            }
        }
    }

    public int DaysTogether
    {
        get => _daysTogether;
        set => SetProperty(ref _daysTogether, value);
    }

    public ObservableCollection<SharedEventDto> UpcomingEvents
    {
        get => _upcomingEvents;
        set => SetProperty(ref _upcomingEvents, value);
    }

    public ObservableCollection<SharedEventDto> MonthEvents
    {
        get => _monthEvents;
        set => SetProperty(ref _monthEvents, value);
    }

    public SharedEventDto? SelectedEvent
    {
        get => _selectedEvent;
        set => SetProperty(ref _selectedEvent, value);
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

    public string CurrentMonthDisplay => CurrentMonth.ToString("MMMM yyyy");

    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand TodayCommand { get; }
    public ICommand CreateEventCommand { get; }
    public ICommand EditEventCommand { get; }
    public ICommand DeleteEventCommand { get; }
    public ICommand RefreshCommand { get; }

    public event Action? OnCreateEventRequested;
    public event Action<SharedEventDto>? OnEditEventRequested;

    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Load days together
            DaysTogether = await _eventService.GetDaysTogetherAsync(_currentUserId);

            // Load upcoming events
            var upcoming = await _eventService.GetUpcomingEventsAsync(_currentUserId, 5);
            UpcomingEvents.Clear();
            foreach (var evt in upcoming)
            {
                UpcomingEvents.Add(evt);
            }

            // Load current month events
            await LoadMonthEventsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMonthEventsAsync()
    {
        try
        {
            var events = await _eventService.GetEventsByMonthAsync(
                _currentUserId,
                CurrentMonth.Year,
                CurrentMonth.Month
            );

            MonthEvents.Clear();
            foreach (var evt in events)
            {
                MonthEvents.Add(evt);
            }

            OnPropertyChanged(nameof(CurrentMonthDisplay));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void NavigateToPreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
    }

    private void NavigateToNextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
    }

    private void NavigateToToday()
    {
        CurrentMonth = DateTime.Now;
    }

    private async Task DeleteEventAsync()
    {
        if (SelectedEvent == null) return;

        try
        {
            await _eventService.DeleteEventAsync(_currentUserId, SelectedEvent.Id);
            await LoadDataAsync();
            SelectedEvent = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public bool HasEventOnDate(DateTime date)
    {
        return MonthEvents.Any(e => e.EventDate.Date == date.Date);
    }

    public IEnumerable<SharedEventDto> GetEventsForDate(DateTime date)
    {
        return MonthEvents.Where(e => e.EventDate.Date == date.Date).OrderBy(e => e.EventDate.TimeOfDay);
    }
}
