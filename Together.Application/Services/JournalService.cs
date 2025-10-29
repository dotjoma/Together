using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class JournalService : IJournalService
{
    private readonly IJournalEntryRepository _journalRepository;
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IStorageService _storageService;
    private readonly IRealTimeSyncService? _realTimeSyncService;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB

    public JournalService(
        IJournalEntryRepository journalRepository,
        ICoupleConnectionRepository connectionRepository,
        IStorageService storageService,
        IRealTimeSyncService? realTimeSyncService = null)
    {
        _journalRepository = journalRepository;
        _connectionRepository = connectionRepository;
        _storageService = storageService;
        _realTimeSyncService = realTimeSyncService;
    }

    public async Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryDto dto)
    {
        // Validate connection exists and user is part of it
        var connection = await _connectionRepository.GetByIdAsync(dto.ConnectionId);
        if (connection == null)
        {
            throw new NotFoundException(nameof(CoupleConnection), dto.ConnectionId);
        }

        if (connection.User1Id != dto.AuthorId && connection.User2Id != dto.AuthorId)
        {
            throw new BusinessRuleViolationException("User is not part of this couple connection");
        }

        // Create journal entry
        var entry = new JournalEntry(dto.ConnectionId, dto.AuthorId, dto.Content, dto.ImageUrl);
        await _journalRepository.AddAsync(entry);

        // Reload with author information
        var createdEntry = await _journalRepository.GetByIdAsync(entry.Id);
        if (createdEntry == null)
        {
            throw new NotFoundException(nameof(JournalEntry), entry.Id);
        }

        var entryDto = MapToDto(createdEntry);

        // Broadcast to partner in real-time
        if (_realTimeSyncService != null)
        {
            try
            {
                await _realTimeSyncService.BroadcastToPartnerAsync("JournalEntryCreated", entryDto);
            }
            catch
            {
                // Don't fail the operation if real-time broadcast fails
            }
        }

        return entryDto;
    }

    public async Task<IEnumerable<JournalEntryDto>> GetJournalEntriesAsync(Guid connectionId)
    {
        var entries = await _journalRepository.GetByConnectionIdAsync(connectionId);
        return entries.Select(MapToDto);
    }

    public async Task MarkAsReadAsync(Guid entryId, Guid userId)
    {
        var entry = await _journalRepository.GetByIdAsync(entryId);
        if (entry == null)
        {
            throw new NotFoundException(nameof(JournalEntry), entryId);
        }

        // Verify user is the partner (not the author)
        var connection = await _connectionRepository.GetByIdAsync(entry.ConnectionId);
        if (connection == null)
        {
            throw new NotFoundException(nameof(CoupleConnection), entry.ConnectionId);
        }

        var partnerId = connection.User1Id == entry.AuthorId ? connection.User2Id : connection.User1Id;
        if (partnerId != userId)
        {
            throw new BusinessRuleViolationException("Only the partner can mark entries as read");
        }

        entry.MarkAsRead();
        await _journalRepository.UpdateAsync(entry);
    }

    public async Task DeleteJournalEntryAsync(Guid entryId, Guid userId)
    {
        var entry = await _journalRepository.GetByIdAsync(entryId);
        if (entry == null)
        {
            throw new NotFoundException(nameof(JournalEntry), entryId);
        }

        // Only the author can delete their entry
        if (entry.AuthorId != userId)
        {
            throw new BusinessRuleViolationException("Only the author can delete their journal entry");
        }

        // Delete image from storage if exists
        if (!string.IsNullOrEmpty(entry.ImageUrl))
        {
            await _storageService.DeleteFileAsync(entry.ImageUrl);
        }

        await _journalRepository.DeleteAsync(entryId);
    }

    public async Task<string?> UploadImageAsync(Guid userId, Stream imageStream, string fileName)
    {
        // Validate file size
        if (imageStream.Length > MaxImageSizeBytes)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Image", new[] { "Image size must not exceed 5MB" } }
            });
        }

        // Upload to storage
        var imageUrl = await _storageService.UploadFileAsync(
            imageStream,
            $"journal/{userId}/{Guid.NewGuid()}_{fileName}",
            "image/jpeg"
        );

        return imageUrl;
    }

    private static JournalEntryDto MapToDto(JournalEntry entry)
    {
        return new JournalEntryDto(
            entry.Id,
            entry.ConnectionId,
            new UserDto(
                entry.Author.Id,
                entry.Author.Username,
                entry.Author.Email.Value,
                entry.Author.ProfilePictureUrl,
                entry.Author.Bio
            ),
            entry.Content ?? string.Empty,
            entry.CreatedAt,
            entry.IsReadByPartner,
            entry.ImageUrl
        );
    }
}
