using System;
using System.Windows.Input;
using Together.Presentation.Commands;
using Together.Services;
using Together.Application.Interfaces;
using Together.Application.DTOs;

namespace Together.Presentation.ViewModels
{
    /// <summary>
    /// Main ViewModel coordinating navigation and application state
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private ViewModelBase? _currentViewModel;
        private UserDto? _currentUser;
        private bool _isNavigationDrawerOpen;

        public MainViewModel(
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            // Subscribe to navigation changes
            _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Initialize commands
            NavigateToCoupleHubCommand = new RelayCommand(_ => NavigateToCoupleHub());
            NavigateToJournalCommand = new RelayCommand(_ => NavigateToJournal());
            NavigateToMoodCommand = new RelayCommand(_ => NavigateToMood());
            NavigateToSocialFeedCommand = new RelayCommand(_ => NavigateToSocialFeed());
            NavigateToProfileCommand = new RelayCommand(_ => NavigateToProfile());
            NavigateToCalendarCommand = new RelayCommand(_ => NavigateToCalendar());
            NavigateToTodoCommand = new RelayCommand(_ => NavigateToTodo());
            NavigateToChallengesCommand = new RelayCommand(_ => NavigateToChallenges());
            NavigateToVirtualPetCommand = new RelayCommand(_ => NavigateToVirtualPet());
            NavigateToLongDistanceCommand = new RelayCommand(_ => NavigateToLongDistance());
            ToggleNavigationDrawerCommand = new RelayCommand(_ => ToggleNavigationDrawer());
            GoBackCommand = new RelayCommand(_ => GoBack(), _ => _navigationService.CanGoBack);
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public UserDto? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public bool IsNavigationDrawerOpen
        {
            get => _isNavigationDrawerOpen;
            set => SetProperty(ref _isNavigationDrawerOpen, value);
        }

        public ICommand NavigateToCoupleHubCommand { get; }
        public ICommand NavigateToJournalCommand { get; }
        public ICommand NavigateToMoodCommand { get; }
        public ICommand NavigateToSocialFeedCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToCalendarCommand { get; }
        public ICommand NavigateToTodoCommand { get; }
        public ICommand NavigateToChallengesCommand { get; }
        public ICommand NavigateToVirtualPetCommand { get; }
        public ICommand NavigateToLongDistanceCommand { get; }
        public ICommand ToggleNavigationDrawerCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand LogoutCommand { get; }

        public void Initialize(UserDto user)
        {
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
            
            // Navigate to Couple Hub by default
            NavigateToCoupleHub();
        }

        private void OnCurrentViewModelChanged(ViewModelBase? viewModel)
        {
            CurrentViewModel = viewModel;
        }

        private void NavigateToCoupleHub()
        {
            _navigationService.NavigateTo<CoupleHubViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToJournal()
        {
            _navigationService.NavigateTo<JournalViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToMood()
        {
            _navigationService.NavigateTo<MoodTrackerViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToSocialFeed()
        {
            _navigationService.NavigateTo<SocialFeedViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToProfile()
        {
            if (CurrentUser != null)
            {
                _navigationService.NavigateTo<UserProfileViewModel>(CurrentUser.Id);
            }
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToCalendar()
        {
            _navigationService.NavigateTo<CalendarViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToTodo()
        {
            _navigationService.NavigateTo<TodoListViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToChallenges()
        {
            _navigationService.NavigateTo<ChallengeViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToVirtualPet()
        {
            _navigationService.NavigateTo<VirtualPetViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void NavigateToLongDistance()
        {
            _navigationService.NavigateTo<LongDistanceViewModel>();
            IsNavigationDrawerOpen = false;
        }

        private void ToggleNavigationDrawer()
        {
            IsNavigationDrawerOpen = !IsNavigationDrawerOpen;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void Logout()
        {
            // Clear navigation history
            _navigationService.ClearHistory();
            
            // Clear current user
            CurrentUser = null;
            
            // Navigate to login (this would need to be handled by App.xaml.cs)
            // For now, we'll just close the application
            System.Windows.Application.Current.Shutdown();
        }
    }
}
