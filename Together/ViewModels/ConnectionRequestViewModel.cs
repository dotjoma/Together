using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class ConnectionRequestViewModel : ViewModelBase
{
    private readonly ICoupleConnectionService _coupleConnectionService;
    private readonly IUserRepository _userRepository;
    private string _searchQuery = string.Empty;
    private string _searchResult = string.Empty;
    private Guid? _selectedUserId;
    private bool _isSearching;
    private bool _isSending;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

    public ConnectionRequestViewModel(
        ICoupleConnectionService coupleConnectionService,
        IUserRepository userRepository)
    {
        _coupleConnectionService = coupleConnectionService;
        _userRepository = userRepository;

        SearchUserCommand = new RelayCommand(async _ => await SearchUserAsync(), _ => !IsSearching && !string.IsNullOrWhiteSpace(SearchQuery));
        SendRequestCommand = new RelayCommand(async _ => await SendRequestAsync(), _ => !IsSending && SelectedUserId.HasValue);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public string SearchResult
    {
        get => _searchResult;
        set => SetProperty(ref _searchResult, value);
    }

    public Guid? SelectedUserId
    {
        get => _selectedUserId;
        set => SetProperty(ref _selectedUserId, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => SetProperty(ref _isSearching, value);
    }

    public bool IsSending
    {
        get => _isSending;
        set => SetProperty(ref _isSending, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public RelayCommand SearchUserCommand { get; }
    public RelayCommand SendRequestCommand { get; }

    private async Task SearchUserAsync()
    {
        try
        {
            IsSearching = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            SearchResult = string.Empty;
            SelectedUserId = null;

            var users = await _userRepository.SearchUsersAsync(SearchQuery, 1);
            var userList = users.ToList();

            if (userList.Count == 0)
            {
                SearchResult = "No user found with that username or email";
            }
            else
            {
                var user = userList[0];
                SelectedUserId = user.Id;
                SearchResult = $"Found: {user.Username} ({user.Email})";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task SendRequestAsync()
    {
        if (!SelectedUserId.HasValue)
            return;

        try
        {
            IsSending = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Get current user ID from session (you'll need to implement session management)
            var currentUserId = GetCurrentUserId();

            await _coupleConnectionService.SendConnectionRequestAsync(currentUserId, SelectedUserId.Value);

            SuccessMessage = "Connection request sent successfully!";
            SearchQuery = string.Empty;
            SearchResult = string.Empty;
            SelectedUserId = null;
        }
        catch (BusinessRuleViolationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (NotFoundException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to send request: {ex.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Implement proper session management
        // For now, this is a placeholder
        return System.Windows.Application.Current.Properties.Contains("CurrentUserId") 
            ? (Guid)System.Windows.Application.Current.Properties["CurrentUserId"]! 
            : Guid.Empty;
    }
}
