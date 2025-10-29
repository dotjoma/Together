using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class JournalViewModel : ViewModelBase
{
    private readonly IJournalService _journalService;
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private readonly IOfflineSyncManager? _offlineSyncManager;
    private readonly Guid _currentUserId;
    private readonly Guid _connectionId;
    private bool _isLoading;
    private string? _errorMessage;
    private JournalEntryViewModel? _entryCreationViewModel;

    public JournalViewModel(
        IJournalService journalService,
        Guid currentUserId,
        Guid connectionId,
        IRealTimeSyncService? realTimeSyncService = null,
        IOfflineSyncManager? offlineSyncManager = null)
    {
        _journalService = journalService;
        _realTimeSyncService = realTimeSyncService;
        _offlineSyncManager = offlineSyncManager;
        _currentUserId = currentUserId;
        _connectionId = connectionId;

        Entries = new ObservableCollection<JournalEntryItemViewModel>();
        
        _entryCreationViewModel = new JournalEntryViewModel(journalService, currentUserId, connectionId);
        _entryCreationViewModel.EntryCreated += OnEntryCreated;

        LoadEntriesCommand = new RelayCommand(async _ => await LoadEntriesAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadEntriesAsync());

        // Subscribe to real-time updates
        if (_realTimeSyncService != null)
        {
            _realTimeSyncService.JournalEntryReceived += OnJournalEntryReceived;
        }

        // Load entries on initialization
        _ = LoadEntriesAsync();
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
