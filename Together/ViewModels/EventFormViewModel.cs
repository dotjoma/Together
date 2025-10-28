using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class EventFormViewModel : ViewModelBase
{
    private readonly IEventService _eventService;
    private readonly Guid _currentUserId;
    private SharedEventDto? _existingEvent;

    private string _title = string.Empty;
    private DateTime _eventDate = DateTime.Now;
    private TimeSpan _eventTime = DateTime.Now.TimeOfDay;
    private string _description = string.Empty;
    private string _selectedRecurrence = "none";
    private bool _isEditMode;
    private string _errorMessage = string.Empty;

    public EventFormViewModel(IEventService eventService, Guid currentUserId)
    {
        _eventService = eventService;
        _currentUserId = currentUserId;

        SaveCommand = new RelayCommand(async _ => await SaveEventAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());

        RecurrenceOptions = new ObservableCollection<string>
        {
            "none",
            "daily",
            "weekly",
            "monthly",
            "yearly"
        };
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public DateTime EventDate
    {
        get => _eventDate;
        set => SetProperty(ref _eventDate, value);
    }

    public TimeSpan EventTime
    {
        get => _eventTime;
        set => SetProperty(ref _eventTime, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string SelectedRecurrence
    {
        get => _selectedRecurrence;
        set => SetProperty(ref _selectedRecurrence, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ObservableCollection<string> RecurrenceOptions { get; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? OnEventSaved;
    public event Action? OnCancelled;

    public void LoadEvent(SharedEventDto eventDto)
    {
        _existingEvent = eventDto;
        IsEditMode = true;

        Title = eventDto.Title;
        EventDate = eventDto.EventDate.Date;
        EventTime = eventDto.EventDate.TimeOfDay;
        Description = eventDto.Description ?? string.Empty;
        SelectedRecurrence = eventDto.Recurrence;
    }

    public void ClearForm()
    {
        _existingEvent = null;
        IsEditMode = false;
        Title = string.Empty;
        EventDate = DateTime.Now;
        EventTime = DateTime.Now.TimeOfDay;
        Description = string.Empty;
        SelectedRecurrence = "none";
        ErrorMessage = string.Empty;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Title);
    }

    private async Task SaveEventAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            var eventDateTime = EventDate.Date + EventTime;

            if (IsEditMode && _existingEvent != null)
            {
                var updateDto = new UpdateEventDto(
                    _existingEvent.Id,
                    Title,
                    eventDateTime,
                    Description,
                    SelectedRecurrence
                );

                await _eventService.UpdateEventAsync(_currentUserId, updateDto);
            }
            else
            {
                var createDto = new CreateEventDto(
                    Title,
                    eventDateTime,
                    Description,
                    SelectedRecurrence
                );

                await _eventService.CreateEventAsync(_currentUserId, createDto);
            }

            OnEventSaved?.Invoke();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
