using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Domain.Enums;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class MoodSelectorViewModel : ViewModelBase
{
    private readonly IMoodTrackingService _moodTrackingService;
    private readonly IOfflineSyncManager? _offlineSyncManager;
    private readonly Guid _userId;
    private string? _notes;
    private string? _selectedMood;
    private bool _isLoading;

    public ObservableCollection<MoodOption> MoodOptions { get; }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string? SelectedMood
    {
        get => _selectedMood;
        set => SetProperty(ref _selectedMood, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand SaveMoodCommand { get; }

    public MoodSelectorViewModel(
        IMoodTrackingService moodTrackingService, 
        Guid userId,
        IOfflineSyncManager? offlineSyncManager = null)
    {
        _moodTrackingService = moodTrackingService;
        _offlineSyncManager = offlineSyncManager;
        _userId = userId;

        MoodOptions = new ObservableCollection<MoodOption>
        {
            new MoodOption("Happy", "😊", "#4CAF50"),
            new MoodOption("Excited", "🤩", "#FF9800"),
            new MoodOption("Calm", "😌", "#2196F3"),
            new MoodOption("Stressed", "😰", "#FFC107"),
            new MoodOption("Anxious", "😟", "#FF5722"),
            new MoodOption("Sad", "😢", "#9C27B0"),
            new MoodOption("Angry", "😠", "#F44336")
        };

        SaveMoodCommand = new RelayCommand(async _ => await SaveMoodAsync(), _ => !string.IsNullOrEmpty(SelectedMood) && !IsLoading);
    }

    private async Task SaveMoodAsync()
    {
        if (string.IsNullOrEmpty(SelectedMood))
            return;

        IsLoading = true;

        try
        {
            var dto = new CreateMoodEntryDto(SelectedMood, Notes);

            // Check if offline
            if (_offlineSyncManager != null && !await _offlineSyncManager.IsOnlineAsync())
            {
                // Queue for later sync
                await _offlineSyncManager.QueueOperationAsync(OperationType.CreateMoodEntry, dto);
                MessageBox.Show("You are offline. Mood will be saved when connection is restored.", 
                    "Queued", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Save immediately
                await _moodTrackingService.CreateMoodEntryAsync(_userId, dto);
                MessageBox.Show("Mood saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Reset form
            SelectedMood = null;
            Notes = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save mood: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class MoodOption
{
    public string Name { get; }
    public string Emoji { get; }
    public string Color { get; }

    public MoodOption(string name, string emoji, string color)
    {
        Name = name;
        Emoji = emoji;
        Color = color;
    }
}
