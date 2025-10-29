namespace Together.Services
{
    /// <summary>
    /// Interface for ViewModels that need to be notified of navigation events
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Called when navigating to this ViewModel
        /// </summary>
        /// <param name="parameter">Optional parameter passed during navigation</param>
        void OnNavigatedTo(object? parameter);

        /// <summary>
        /// Called when navigating away from this ViewModel
        /// </summary>
        void OnNavigatedFrom();
    }
}
