using Together.Presentation.ViewModels;

namespace Together.Services
{
    /// <summary>
    /// Service for managing navigation between views in the application
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the current ViewModel being displayed
        /// </summary>
        ViewModelBase? CurrentViewModel { get; }

        /// <summary>
        /// Event raised when the current ViewModel changes
        /// </summary>
        event Action<ViewModelBase?>? CurrentViewModelChanged;

        /// <summary>
        /// Navigates to a ViewModel of the specified type
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel to navigate to</typeparam>
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates to a ViewModel of the specified type with a parameter
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel to navigate to</typeparam>
        /// <param name="parameter">Parameter to pass to the ViewModel</param>
        void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase;

        /// <summary>
        /// Navigates back to the previous ViewModel
        /// </summary>
        void GoBack();

        /// <summary>
        /// Gets whether navigation back is possible
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Clears the navigation history
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Registers a ViewModel type with the navigation service
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type to register</typeparam>
        void RegisterViewModel<TViewModel>() where TViewModel : ViewModelBase;
    }
}
