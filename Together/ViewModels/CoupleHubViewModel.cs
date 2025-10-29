using System.Collections.ObjectModel;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class CoupleHubViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly Guid _currentUserId;

    private MoodEntryDto? _partnerMood;
    private int _loveStreak;
    private VirtualPetDto? _virtualPet;
    private int _daysTogether;
    private string? _supportiveMessage;
    private DailySuggestionDto? _dailySuggestion;
    private bool _isLoading;
    private string? _errorMessage;

    public CoupleHubViewModel(IDashboardService dashboardService, Guid currentUserId)
    {
        _dashboardService = dashboardService;
        _currentUserId = currentUserId;

        UpcomingEvents = new ObservableCollection<SharedEventDto>();
        TogetherMoments = new ObservableCollection<TogetherMomentDto>();

        RefreshCommand = new RelayCommand(async _ => await LoadDashboardDataAsync());

        // Load data on initialization
        _ = LoadDashboardDataAsync();
    }

    public MoodEntryDto? PartnerMood
    {
        get => _partnerMood;
        set => SetProperty(ref _partnerMood, value);
    }

    public int LoveStreak
    {
        get => _loveStreak;
        set => SetProperty(ref _loveStreak, value);
    }

    public VirtualPetDto? VirtualPet
    {
        get => _virtualPet;
        set => SetProperty(ref _virtualPet, value);
    }

    public int DaysTogether
    {
        get => _daysTogether;
        set => SetProperty(ref _daysTogether, value);
    }

    public string? SupportiveMessage
    {
        get => _supportiveMessage;
        set => SetProperty(ref _supportiveMessage, value);
    }

    public DailySuggestionDto? DailySuggestion
    {
        get => _dailySuggestion;
        set => SetProperty(ref _dailySuggestion, value);
    }

    public ObservableCollection<SharedEventDto> UpcomingEvents { get; }

    public ObservableCollection<TogetherMomentDto> TogetherMoments { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RefreshCommand { get; }

    private async Task LoadDashboardDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            // Load dashboard summary
            var summary = await _dashboardService.GetDashboardSummaryAsync(_currentUserId);

            PartnerMood = summary.PartnerMood;
            LoveStreak = summary.LoveStreak;
            VirtualPet = summary.VirtualPet;
            DaysTogether = summary.DaysTogether;
            SupportiveMessage = summary.SupportiveMessage;

            // Load upcoming events
            UpcomingEvents.Clear();
            foreach (var evt in summary.UpcomingEvents)
            {
                UpcomingEvents.Add(evt);
            }

            // Load together moments
            var moments = await _dashboardService.GetTogetherMomentsAsync(_currentUserId, 5);
            TogetherMoments.Clear();
            foreach (var moment in moments)
            {
                TogetherMoments.Add(moment);
            }

            // Load daily suggestion
            DailySuggestion = await _dashboardService.GetDailySuggestionAsync(_currentUserId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
