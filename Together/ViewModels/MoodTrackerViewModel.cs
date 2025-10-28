using System.Windows.Input;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class MoodTrackerViewModel : ViewModelBase
{
    private readonly Guid _userId;
    private bool _showSelector = true;

    public MoodSelectorViewModel MoodSelector { get; }
    public MoodHistoryViewModel MoodHistory { get; }

    public bool ShowSelector
    {
        get => _showSelector;
        set => SetProperty(ref _showSelector, value);
    }

    public bool ShowHistory => !ShowSelector;

    public ICommand ShowSelectorCommand { get; }
    public ICommand ShowHistoryCommand { get; }
    public ICommand RefreshCommand { get; }

    public MoodTrackerViewModel(
        IMoodTrackingService moodTrackingService,
        IMoodAnalysisService moodAnalysisService,
        Guid userId)
    {
        _userId = userId;

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
    }
}
