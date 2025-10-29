using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Services;

namespace Together.Presentation.ViewModels;

public class JournalViewModel : ViewModelBase, INavigationAware
{
    private readonly IJournalService _journalService;
    private readonly ICoupleConnectionService _coupleConnectionService;
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private readonly IOfflineSyncManager? _offlineSyncManager;
    private Guid _currentUserId;
    private Guid _connectionId;
    private bool _isLoading;
    private string? _errorMessage;
    private JournalEntryViewModel? _entryCreationViewModel;

    public JournalViewModel(
        IJournalService journalService,
        ICoupleConnectionService coupleConnectionService,
        IRealTimeSyncService? realTimeSyncService = null,
        IOfflineSyncManager? offlineSyncManager = null)
    {
        _journalService = journalService;
        _coupleConnectionService = coupleConnectionService;
        _realTimeSyncService = realTimeSyncService;
        _offlineSyncManager = offlineSyncManager;

        Entries = new ObservableCollection<JournalEntryItemViewModel>();

        LoadEntriesCommand = new RelayCommand(async _ => await LoadEntriesAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadEntriesAsync());

        // Subscribe to real-time updates
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.JournalEntryReceived += OnJournalEntryReceived;
        }
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
                
                // Initialize entry creation view model
                _entryCreationViewModel = new JournalEntryViewModel(_journalService, _currentUserId, _connectionId);
                _entryCreationViewModel.EntryCreated += OnEntryCreated;
                
                // Load entries
                await LoadEntriesAsync();
            }
        }
    }

    public void OnNavigatedFrom()
    {
        // Cleanup
        if (_entryCreationViewModel != null)
        {
            _entryCreationViewModel.EntryCreated -= OnEntryCreated;
        }
    }
    }

    public ObservableCollection<JournalEntryItemViewModel> Entries { get; }

    public JournalEntryViewModel? EntryCreationViewModel
    {
        get => _entryCreationViewModel;
        set => SetProperty(ref _entryCreationViewModel, value);
    }

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

    public ICommand LoadEntriesCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadEntriesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            IEnumerable<JournalEntryDto> entries;

            // Try to load from server, fall back to cache if offline
            if (_offlineSyncManager != null && !await _offlineSyncManager.IsOnlineAsync())
            {
                var cachedEntries = await _offlineSyncManager.GetCachedJournalEntriesAsync(_connectionId);
                entries = cachedEntries.Cast<JournalEntryDto>();
            }
            else
            {
                entries = await _journalService.GetJournalEntriesAsync(_connectionId);
                
                // Cache the entries for offline use
                if (_offlineSyncManager != null)
                {
                    await _offlineSyncManager.CacheJournalEntriesAsync(_connectionId, entries.Cast<object>());
                }
            }
            
            Entries.Clear();
            foreach (var entry in entries)
            {
                var viewModel = new JournalEntryItemViewModel(
                    entry,
                    _journalService,
                    _currentUserId
                );
                viewModel.EntryDeleted += OnEntryDeleted;
                Entries.Add(viewModel);
            }

            // Mark unread entries as read
            await MarkUnreadEntriesAsReadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task MarkUnreadEntriesAsReadAsync()
    {
        var unreadEntries = Entries
            .Where(e => !e.IsReadByPartner && e.AuthorId != _currentUserId)
            .ToList();

        foreach (var entry in unreadEntries)
        {
            try
            {
                await _journalService.MarkAsReadAsync(entry.Id, _currentUserId);
                entry.MarkAsRead();
            }
            catch
            {
                // Silently fail for individual mark as read operations
            }
        }
    }

    private void OnEntryCreated(object? sender, JournalEntryDto entry)
    {
        var viewModel = new JournalEntryItemViewModel(
            entry,
            _journalService,
            _currentUserId
        );
        viewModel.EntryDeleted += OnEntryDeleted;
        
        // Add to the beginning of the list (most recent first)
        Entries.Insert(0, viewModel);
    }

    private void OnEntryDeleted(object? sender, Guid entryId)
    {
        var entry = Entries.FirstOrDefault(e => e.Id == entryId);
        if (entry != null)
        {
            entry.EntryDeleted -= OnEntryDeleted;
            Entries.Remove(entry);
        }
    }

    private void OnJournalEntryReceived(object? sender, JournalEntryDto entry)
    {
        // Only add entries from partner (not our own, as they're already added locally)
        if (entry.Author.Id == _currentUserId)
            return;

        // Check if entry already exists
        if (Entries.Any(e => e.Id == entry.Id))
            return;

        // Add the new entry to the collection on UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            var viewModel = new JournalEntryItemViewModel(
                entry,
                _journalService,
                _currentUserId
            );
            viewModel.EntryDeleted += OnEntryDeleted;
            Entries.Insert(0, viewModel);
        });
    }

    public void Dispose()
    {
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.JournalEntryReceived -= OnJournalEntryReceived;
        }

        if (_entryCreationViewModel != null)
        {
            _entryCreationViewModel.EntryCreated -= OnEntryCreated;
        }

        foreach (var entry in Entries)
        {
            entry.EntryDeleted -= OnEntryDeleted;
        }
    }
}
