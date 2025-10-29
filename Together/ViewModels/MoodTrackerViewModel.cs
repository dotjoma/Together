using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Services;

namespace Together.Presentation.ViewModels;

public class MoodTrackerViewModel : ViewModelBase, INavigationAware
{
    private readonly IMoodTrackingService _moodTrackingService;
    private readonly IMoodAnalysisService _moodAnalysisService;
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private Guid _userId;
    private bool _showSelector = true;
    private string? _partnerMoodStatus;
    private MoodSelectorViewModel? _moodSelector;
    private MoodHistoryViewModel? _moodHistory;

    public MoodSelectorViewModel? MoodSelector
    {
        get => _moodSelector;
        private set => SetProperty(ref _moodSelector, value);
    }

    public MoodHistoryViewModel? MoodHistory
    {
        get => _moodHistory;
        private set => SetProperty(ref _moodHistory, value);
    }

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
        IRealTimeSyncService? realTimeSyncService = null)
    {
        _moodTrackingService = moodTrackingService;
        _moodAnalysisService = moodAnalysisService;
        _realTimeSyncService = realTimeSyncService;

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

        RefreshCommand = new RelayCommand(async _ =>
        {
            if (MoodHistory != null)
            {
                await MoodHistory.RefreshAsync();
            }
        });

        // Subscribe to real-time mood updates
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.MoodEntryReceived += OnMoodEntryReceived;
        }
    }

    public void OnNavigatedTo(object? parameter)
    {
        // Get current user from application properties
        var currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as UserDto;
        if (currentUser != null)
        {
            _userId = currentUser.Id;
            MoodSelector = new MoodSelectorViewModel(_moodTrackingService, _userId);
            MoodHistory = new MoodHistoryViewModel(_moodTrackingService, _moodAnalysisService, _userId);
        }
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if needed
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
            _ = MoodHistory?.RefreshAsync();
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
