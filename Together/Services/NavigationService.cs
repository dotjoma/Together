using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Together.ViewModels;

namespace Together.Services
{
    /// <summary>
    /// Implementation of navigation service for ViewModel-based navigation
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<ViewModelBase> _navigationHistory;
        private readonly Dictionary<Type, Type> _viewModelRegistry;
        private ViewModelBase? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _navigationHistory = new Stack<ViewModelBase>();
            _viewModelRegistry = new Dictionary<Type, Type>();
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    CurrentViewModelChanged?.Invoke(_currentViewModel);
                }
            }
        }

        public event Action<ViewModelBase?>? CurrentViewModelChanged;

        public bool CanGoBack => _navigationHistory.Count > 0;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object? parameter) where TViewModel : ViewModelBase
        {
            try
            {
                // Save current view model to history if it exists
                if (_currentViewModel != null)
                {
                    _navigationHistory.Push(_currentViewModel);
                }

                // Create new view model instance
                var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

                // Pass parameter if the ViewModel supports it
                if (parameter != null && viewModel is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(parameter);
                }

                CurrentViewModel = viewModel;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to navigate to {typeof(TViewModel).Name}. Ensure the ViewModel is registered in the DI container.", 
                    ex);
            }
        }

        public void GoBack()
        {
            if (!CanGoBack)
            {
                return;
            }

            var previousViewModel = _navigationHistory.Pop();
            
            // Notify current view model it's being navigated away from
            if (_currentViewModel is INavigationAware currentNavigationAware)
            {
                currentNavigationAware.OnNavigatedFrom();
            }

            CurrentViewModel = previousViewModel;

            // Notify previous view model it's being navigated back to
            if (previousViewModel is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(null);
            }
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        public void RegisterViewModel<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModelType = typeof(TViewModel);
            if (!_viewModelRegistry.ContainsKey(viewModelType))
            {
                _viewModelRegistry.Add(viewModelType, viewModelType);
            }
        }
    }
}
