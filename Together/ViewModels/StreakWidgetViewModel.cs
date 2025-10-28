using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Together.Application.Interfaces;

namespace Together.Presentation.ViewModels;

public class StreakWidgetViewModel : ViewModelBase
{
    private readonly ILoveStreakService _loveStreakService;
    private readonly Guid _connectionId;
    private int _currentStreak;
    private List<int> _achievedMilestones;
    private bool _isLoading;
    private string _streakMessage;
    private bool _showCelebration;

    public StreakWidgetViewModel(ILoveStreakService loveStreakService, Guid connectionId)
    {
        _loveStreakService = loveStreakService;
        _connectionId = connectionId;
        _achievedMilestones = new List<int>();
        _streakMessage = "Keep the streak alive!";
        
        _ = LoadStreakDataAsync();
    }

    public int CurrentStreak
    {
        get => _currentStreak;
        set
        {
            if (SetProperty(ref _currentStreak, value))
            {
                OnPropertyChanged(nameof(StreakDisplay));
                OnPropertyChanged(nameof(NextMilestone));
                OnPropertyChanged(nameof(ProgressToNextMilestone));
            }
        }
    }

    public List<int> AchievedMilestones
    {
        get => _achievedMilestones;
        set => SetProperty(ref _achievedMilestones, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StreakMessage
    {
        get => _streakMessage;
        set => SetProperty(ref _streakMessage, value);
    }

    public bool ShowCelebration
    {
        get => _showCelebration;
        set => SetProperty(ref _showCelebration, value);
    }

    public string StreakDisplay => $"{CurrentStreak} {(CurrentStreak == 1 ? "Day" : "Days")}";

    public int NextMilestone
    {
        get
        {
            var milestones = new[] { 7, 30, 100, 365 };
            return milestones.FirstOrDefault(m => m > CurrentStreak);
        }
    }

    public double ProgressToNextMilestone
    {
        get
        {
            if (NextMilestone == 0) return 100.0; // All milestones achieved
            
            var previousMilestone = 0;
            if (CurrentStreak >= 7) previousMilestone = 7;
            if (CurrentStreak >= 30) previousMilestone = 30;
            if (CurrentStreak >= 100) previousMilestone = 100;
            
            var range = NextMilestone - previousMilestone;
            var progress = CurrentStreak - previousMilestone;
            
            return range > 0 ? (progress / (double)range) * 100.0 : 0.0;
        }
    }

    public async Task LoadStreakDataAsync()
    {
        IsLoading = true;
        try
        {
            CurrentStreak = await _loveStreakService.GetCurrentStreakAsync(_connectionId);
            var milestones = await _loveStreakService.GetStreakMilestonesAsync(_connectionId);
            AchievedMilestones = milestones.ToList();
            
            UpdateStreakMessage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading streak data: {ex.Message}");
            StreakMessage = "Unable to load streak data";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshStreakAsync()
    {
        await LoadStreakDataAsync();
    }

    public void TriggerCelebration(int milestone)
    {
        ShowCelebration = true;
        StreakMessage = milestone switch
        {
            7 => "🎉 Amazing! 7-day streak!",
            30 => "🌟 Incredible! 30-day streak!",
            100 => "💯 Wow! 100-day streak!",
            365 => "🏆 Legendary! 365-day streak!",
            _ => $"🎊 {milestone}-day milestone!"
        };
        
        // Hide celebration after 5 seconds
        Task.Delay(5000).ContinueWith(_ => ShowCelebration = false);
    }

    private void UpdateStreakMessage()
    {
        if (CurrentStreak == 0)
        {
            StreakMessage = "Start your love streak today! 💕";
        }
        else if (CurrentStreak < 7)
        {
            StreakMessage = $"Keep going! {7 - CurrentStreak} days to your first milestone!";
        }
        else if (CurrentStreak < 30)
        {
            StreakMessage = $"Great job! {30 - CurrentStreak} days to 30-day milestone!";
        }
        else if (CurrentStreak < 100)
        {
            StreakMessage = $"Amazing! {100 - CurrentStreak} days to 100-day milestone!";
        }
        else if (CurrentStreak < 365)
        {
            StreakMessage = $"Incredible! {365 - CurrentStreak} days to 1-year milestone!";
        }
        else
        {
            StreakMessage = "🏆 You've achieved all milestones! Keep the streak alive!";
        }
    }
}
