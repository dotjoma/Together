using System;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class JournalEntryItemViewModel : ViewModelBase
{
    private readonly IJournalService _journalService;
    private readonly Guid _currentUserId;
    private bool _isReadByPartner;
    private bool _isDeleting;

    public JournalEntryItemViewModel(
        JournalEntryDto entry,
        IJournalService journalService,
        Guid currentUserId)
    {
        _journalService = journalService;
        _currentUserId = currentUserId;

        Id = entry.Id;
        ConnectionId = entry.ConnectionId;
        AuthorId = entry.Author.Id;
        AuthorName = entry.Author.Username;
        AuthorProfilePicture = entry.Author.ProfilePictureUrl;
        Content = entry.Content;
        CreatedAt = entry.CreatedAt;
        _isReadByPartner = entry.IsReadByPartner;
        ImageUrl = entry.ImageUrl;

        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => CanDelete());
    }

    public Guid Id { get; }
    public Guid ConnectionId { get; }
    public Guid AuthorId { get; }
    public string AuthorName { get; }
    public string? AuthorProfilePicture { get; }
    public string Content { get; }
    public DateTime CreatedAt { get; }
    public string? ImageUrl { get; }

    public bool IsReadByPartner
    {
        get => _isReadByPartner;
        private set => SetProperty(ref _isReadByPartner, value);
    }

    public bool IsDeleting
    {
        get => _isDeleting;
        set => SetProperty(ref _isDeleting, value);
    }

    public bool IsOwnEntry => AuthorId == _currentUserId;

    public string FormattedDate => CreatedAt.ToLocalTime().ToString("MMM dd, yyyy 'at' h:mm tt");

    public ICommand DeleteCommand { get; }

    public event EventHandler<Guid>? EntryDeleted;

    public void MarkAsRead()
    {
        IsReadByPartner = true;
    }

    private bool CanDelete()
    {
        return IsOwnEntry && !IsDeleting;
    }

    private async Task DeleteAsync()
    {
        try
        {
            IsDeleting = true;
            await _journalService.DeleteJournalEntryAsync(Id, _currentUserId);
            EntryDeleted?.Invoke(this, Id);
        }
        catch
        {
            IsDeleting = false;
            // Error handling could be improved with user notification
        }
    }
}
