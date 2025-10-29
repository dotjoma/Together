using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class MoodTrackerViewModel : ViewModelBase
{
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private readonly Guid _userId;
    private bool _showSelector = true;
    private string? _partnerMoodStatus;

    public MoodSelectorViewModel MoodSelector { get; }
    public MoodHistoryViewModel MoodHistory { get; }

    public bool ShowSelector
    {
        get => _showSelector;
        set => SetProperty(ref _showSelector, value);
    }

    public bool ShowHistory => !ShowSelector;

    public string? PartnerMoodStatus
    {
        get => _partnerMoodStatus;
        set => SetProperty(ref _partnerMoodStatus, value);
    }

    public ICommand ShowSelectorCommand { get; }
    public ICommand ShowHistoryCommand { get; }
    public ICommand RefreshCommand { get; }

    public MoodTrackerViewModel(
        IMoodTrackingService moodTrackingService,
        IMoodAnalysisService moodAnalysisService,
        Guid userId,
        IRealTimeSyncService? realTimeSyncService = null)
    {
        _userId = userId;
        _realTimeSyncService = realTimeSyncService;

        MoodSelector = new MoodSelectorViewModel(moodTrackingService, userId);
        MoodHistory = new MoodHistoryViewModel(moodTrackingService, moodAnalysisService, userId);

        ShowSelectorCommand = new RelayCommand(_ =>
        {
            ShowSelector = true;
            OnPropertyChanged(nameof(ShowHistory));
        });

        ShowHistoryCommand = new RelayCommand(_ =>
        {
            ShowSelector = false;
            OnPropertyChanged(nameof(ShowHistory));
        });

        RefreshCommand = new RelayCommand(async _ => await MoodHistory.RefreshAsync());

        // Subscribe to real-time mood updates
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.MoodEntryReceived += OnMoodEntryReceived;
        }
    }

    private void OnMoodEntryReceived(object? sender, MoodEntryDto moodEntry)
    {
        // Only show partner's mood updates
        if (moodEntry.UserId == _userId)
            return;

        // Update partner mood status on UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            PartnerMoodStatus = $"Your partner is feeling {moodEntry.Mood.ToLower()}";
            
            // Refresh mood history to include the new entry
            _ = MoodHistory.RefreshAsync();
        });
    }

    public void Dispose()
    {
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.MoodEntryReceived -= OnMoodEntryReceived;
        }
    }
}
