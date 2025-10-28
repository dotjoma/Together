using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Domain.Enums;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class StreakHistoryViewModel : ViewModelBase
{
    private readonly ILoveStreakService _loveStreakService;
    private readonly ICoupleConnectionService _coupleConnectionService;
    private readonly Guid _currentUserId;
    private StreakWidgetViewModel? _streakWidget;
    private ObservableCollection<InteractionHistoryItem> _interactionHistory;
    private bool _isLoading;

    public StreakHistoryViewModel(
        ILoveStreakService loveStreakService,
        ICoupleConnectionService coupleConnectionService,
        Guid currentUserId)
    {
        _loveStreakService = loveStreakService;
        _coupleConnectionService = coupleConnectionService;
        _currentUserId = currentUserId;
        _interactionHistory = new ObservableCollection<InteractionHistoryItem>();

        RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
        
        _ = InitializeAsync();
    }

    public StreakWidgetViewModel? StreakWidget
    {
        get => _streakWidget;
        set => SetProperty(ref _streakWidget, value);
    }

    public ObservableCollection<InteractionHistoryItem> InteractionHistory
    {
        get => _interactionHistory;
        set => SetProperty(ref _interactionHistory, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand RefreshCommand { get; }

    private async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var connection = await _coupleConnectionService.GetUserConnectionAsync(_currentUserId);
            if (connection != null)
            {
                StreakWidget = new StreakWidgetViewModel(_loveStreakService, connection.Id);
                await LoadInteractionHistoryAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing streak history: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        if (StreakWidget != null)
        {
            await StreakWidget.RefreshStreakAsync();
            await LoadInteractionHistoryAsync();
        }
    }

    private async Task LoadInteractionHistoryAsync()
    {
        // This is a placeholder - in a real implementation, you would fetch actual interaction history
        // For now, we'll create sample data to demonstrate the UI
        await Task.CompletedTask;
        
        InteractionHistory.Clear();
        
        // Sample data showing different interaction types
        var sampleInteractions = new[]
        {
            new InteractionHistoryItem(DateTime.UtcNow.AddHours(-2), InteractionType.JournalEntry, "Shared a journal entry"),
            new InteractionHistoryItem(DateTime.UtcNow.AddHours(-5), InteractionType.MoodLog, "Logged mood"),
            new InteractionHistoryItem(DateTime.UtcNow.AddDays(-1).AddHours(-3), InteractionType.TodoCompletion, "Completed a todo"),
            new InteractionHistoryItem(DateTime.UtcNow.AddDays(-1).AddHours(-8), InteractionType.JournalEntry, "Shared a journal entry"),
            new InteractionHistoryItem(DateTime.UtcNow.AddDays(-2).AddHours(-4), InteractionType.ChallengeCompletion, "Completed a challenge"),
        };

        foreach (var interaction in sampleInteractions)
        {
            InteractionHistory.Add(interaction);
        }
    }
}

public class InteractionHistoryItem
{
    public InteractionHistoryItem(DateTime timestamp, InteractionType type, string description)
    {
        Timestamp = timestamp;
        Type = type;
        Description = description;
    }

    public DateTime Timestamp { get; }
    public InteractionType Type { get; }
    public string Description { get; }
    
    public string TypeIcon => Type switch
    {
        InteractionType.JournalEntry => "BookOpen",
        InteractionType.MoodLog => "EmoticonHappy",
        InteractionType.ChatMessage => "Message",
        InteractionType.ChallengeCompletion => "Trophy",
        InteractionType.TodoCompletion => "CheckCircle",
        _ => "Circle"
    };

    public string TypeColor => Type switch
    {
        InteractionType.JournalEntry => "#6366F1",
        InteractionType.MoodLog => "#F59E0B",
        InteractionType.ChatMessage => "#10B981",
        InteractionType.ChallengeCompletion => "#EF4444",
        InteractionType.TodoCompletion => "#8B5CF6",
        _ => "#6B7280"
    };

    public string RelativeTime
    {
        get
        {
            var timeSpan = DateTime.UtcNow - Timestamp;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            return Timestamp.ToString("MMM dd");
        }
    }
}
