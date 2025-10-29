using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Services;

namespace Together.Presentation.ViewModels;

public class ChallengeViewModel : ViewModelBase, INavigationAware
{
    private readonly IChallengeService _challengeService;
    private readonly ICoupleConnectionService _coupleConnectionService;
    private Guid _currentUserId;
    private Guid _connectionId;

    private ObservableCollection<ChallengeDto> _activeChallenges;
    private ChallengeDto? _selectedChallenge;
    private int _coupleScore;
    private bool _isLoading;
    private string _errorMessage;
    private bool _hasNoChallenges;

    public ChallengeViewModel(IChallengeService challengeService, ICoupleConnectionService coupleConnectionService)
    {
        _challengeService = challengeService;
        _coupleConnectionService = coupleConnectionService;
        _activeChallenges = new ObservableCollection<ChallengeDto>();
        _errorMessage = string.Empty;

        CompleteChallengeCommand = new RelayCommand(async _ => await CompleteChallengeAsync(), _ => SelectedChallenge != null);
        GenerateNewChallengeCommand = new RelayCommand(async _ => await GenerateNewChallengeAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        SelectChallengeCommand = new RelayCommand(param => SelectChallenge(param as ChallengeDto));
    }

    public async void OnNavigatedTo(object? parameter)
    {
        // Get current user from application properties
        var currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as UserDto;
        if (currentUser != null)
        {
            _currentUserId = currentUser.Id;
            
            // Get couple connection
            var connection = await _coupleConnectionService.GetUserConnectionAsync(_currentUserId);
            if (connection != null)
            {
                _connectionId = connection.Id;
                await LoadDataAsync();
            }
        }
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if needed
    }

    public ObservableCollection<ChallengeDto> ActiveChallenges
    {
        get => _activeChallenges;
        set => SetProperty(ref _activeChallenges, value);
    }

    public ChallengeDto? SelectedChallenge
    {
        get => _selectedChallenge;
        set => SetProperty(ref _selectedChallenge, value);
    }

    public int CoupleScore
    {
        get => _coupleScore;
        set => SetProperty(ref _coupleScore, value);
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

    public bool HasNoChallenges
    {
        get => _hasNoChallenges;
        set => SetProperty(ref _hasNoChallenges, value);
    }

    public ICommand CompleteChallengeCommand { get; }
    public ICommand GenerateNewChallengeCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SelectChallengeCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var challenges = await _challengeService.GetActiveChallengesAsync(_connectionId);
            ActiveChallenges = new ObservableCollection<ChallengeDto>(challenges);
            HasNoChallenges = !ActiveChallenges.Any();

            CoupleScore = await _challengeService.GetCoupleScoreAsync(_connectionId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load challenges: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SelectChallenge(ChallengeDto? challenge)
    {
        SelectedChallenge = challenge;
    }

    private async Task CompleteChallengeAsync()
    {
        if (SelectedChallenge == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var updatedChallenge = await _challengeService.CompleteChallengeAsync(SelectedChallenge.Id, _currentUserId);

            // Update the challenge in the collection
            var index = ActiveChallenges.IndexOf(SelectedChallenge);
            if (index >= 0)
            {
                ActiveChallenges[index] = updatedChallenge;
            }

            // Refresh score
            CoupleScore = await _challengeService.GetCoupleScoreAsync(_connectionId);

            if (updatedChallenge.IsFullyCompleted)
            {
                MessageBox.Show($"Challenge completed! You earned {updatedChallenge.Points} points!", 
                    "Challenge Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Challenge marked as complete! Waiting for your partner.", 
                    "Progress Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to complete challenge: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GenerateNewChallengeAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var newChallenge = await _challengeService.GenerateDailyChallengeAsync(_connectionId);
            
            // Check if it's a new challenge or existing one
            if (!ActiveChallenges.Any(c => c.Id == newChallenge.Id))
            {
                ActiveChallenges.Insert(0, newChallenge);
                HasNoChallenges = false;
                MessageBox.Show("New daily challenge generated!", "New Challenge", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Today's challenge already exists!", "Info", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to generate challenge: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
