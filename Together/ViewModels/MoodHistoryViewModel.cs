using System.Collections.ObjectModel;
using System.Windows;
using Together.Application.DTOs;
using Together.Application.Interfaces;

namespace Together.Presentation.ViewModels;

public class MoodHistoryViewModel : ViewModelBase
{
    private readonly IMoodTrackingService _moodTrackingService;
    private readonly IMoodAnalysisService _moodAnalysisService;
    private readonly Guid _userId;
    private bool _isLoading;
    private MoodTrendDto? _moodTrend;

    public ObservableCollection<MoodEntryDto> MoodHistory { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public MoodTrendDto? MoodTrend
    {
        get => _moodTrend;
        set => SetProperty(ref _moodTrend, value);
    }

    public string TrendDescription => MoodTrend != null
        ? $"Your mood trend over the past 30 days is {MoodTrend.TrendType} (Average: {MoodTrend.AverageScore:F1}/5)"
        : "No mood data available";

    public MoodHistoryViewModel(IMoodTrackingService moodTrackingService, IMoodAnalysisService moodAnalysisService, Guid userId)
    {
        _moodTrackingService = moodTrackingService;
        _moodAnalysisService = moodAnalysisService;
        _userId = userId;

        MoodHistory = new ObservableCollection<MoodEntryDto>();

        _ = LoadMoodHistoryAsync();
    }

    private async Task LoadMoodHistoryAsync()
    {
        IsLoading = true;

        try
        {
            var history = await _moodTrackingService.GetMoodHistoryAsync(_userId, 30);
            MoodHistory.Clear();
            foreach (var entry in history)
            {
                MoodHistory.Add(entry);
            }

            var trend = await _moodAnalysisService.AnalyzeMoodTrendAsync(_userId, 30);
            MoodTrend = trend;
            OnPropertyChanged(nameof(TrendDescription));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load mood history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshAsync()
    {
        await LoadMoodHistoryAsync();
    }
}
