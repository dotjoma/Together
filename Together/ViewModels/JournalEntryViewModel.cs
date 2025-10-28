using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class JournalEntryViewModel : ViewModelBase
{
    private readonly IJournalService _journalService;
    private readonly Guid _currentUserId;
    private readonly Guid _connectionId;
    private string _content = string.Empty;
    private string? _imageUrl;
    private bool _isCreating;
    private string? _errorMessage;

    public JournalEntryViewModel(
        IJournalService journalService,
        Guid currentUserId,
        Guid connectionId)
    {
        _journalService = journalService;
        _currentUserId = currentUserId;
        _connectionId = connectionId;

        CreateEntryCommand = new RelayCommand(async _ => await CreateEntryAsync(), _ => CanCreateEntry());
        SelectImageCommand = new RelayCommand(_ => SelectImage());
        RemoveImageCommand = new RelayCommand(_ => RemoveImage());
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public string? ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    public bool IsCreating
    {
        get => _isCreating;
        set => SetProperty(ref _isCreating, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand CreateEntryCommand { get; }
    public ICommand SelectImageCommand { get; }
    public ICommand RemoveImageCommand { get; }

    public event EventHandler<JournalEntryDto>? EntryCreated;

    private bool CanCreateEntry()
    {
        return !string.IsNullOrWhiteSpace(Content) && !IsCreating;
    }

    private async Task CreateEntryAsync()
    {
        try
        {
            IsCreating = true;
            ErrorMessage = null;

            var dto = new CreateJournalEntryDto(
                _connectionId,
                _currentUserId,
                Content,
                ImageUrl
            );

            var createdEntry = await _journalService.CreateJournalEntryAsync(dto);
            
            // Clear form
            Content = string.Empty;
            ImageUrl = null;

            // Notify listeners
            EntryCreated?.Invoke(this, createdEntry);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsCreating = false;
        }
    }

    private void SelectImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
            Title = "Select Journal Image"
        };

        if (dialog.ShowDialog() == true)
        {
            UploadImageAsync(dialog.FileName);
        }
    }

    private async void UploadImageAsync(string filePath)
    {
        try
        {
            IsCreating = true;
            ErrorMessage = null;

            using var stream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);
            
            ImageUrl = await _journalService.UploadImageAsync(_currentUserId, stream, fileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsCreating = false;
        }
    }

    private void RemoveImage()
    {
        ImageUrl = null;
    }
}
