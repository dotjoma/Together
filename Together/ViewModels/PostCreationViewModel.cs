using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class PostCreationViewModel : ViewModelBase
{
    private readonly IPostService _postService;
    private readonly IOfflineSyncManager? _offlineSyncManager;
    private readonly Guid _currentUserId;
    private string _content = string.Empty;
    private int _characterCount;
    private bool _isPosting;
    private string? _errorMessage;
    private PostDto? _editingPost;

    public PostCreationViewModel(
        IPostService postService, 
        Guid currentUserId,
        IOfflineSyncManager? offlineSyncManager = null)
    {
        _postService = postService;
        _offlineSyncManager = offlineSyncManager;
        _currentUserId = currentUserId;
        
        ImagePaths = new ObservableCollection<string>();
        
        PostCommand = new RelayCommand(async _ => await PostAsync(), _ => CanPost());
        AddImageCommand = new RelayCommand(_ => AddImage(), _ => CanAddImage());
        RemoveImageCommand = new RelayCommand(RemoveImage);
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                CharacterCount = value?.Length ?? 0;
                ErrorMessage = null;
            }
        }
    }

    public int CharacterCount
    {
        get => _characterCount;
        private set => SetProperty(ref _characterCount, value);
    }

    public int MaxCharacters => 500;

    public int RemainingCharacters => MaxCharacters - CharacterCount;

    public ObservableCollection<string> ImagePaths { get; }

    public bool IsPosting
    {
        get => _isPosting;
        private set => SetProperty(ref _isPosting, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsEditing => _editingPost != null;

    public ICommand PostCommand { get; }
    public ICommand AddImageCommand { get; }
    public ICommand RemoveImageCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler? PostCreated;
    public event EventHandler? PostUpdated;
    public event EventHandler? Cancelled;

    public void SetEditMode(PostDto post)
    {
        _editingPost = post;
        Content = post.Content;
        OnPropertyChanged(nameof(IsEditing));
    }

    public bool CanEdit(PostDto post)
    {
        var timeSinceCreation = DateTime.UtcNow - post.CreatedAt;
        return timeSinceCreation.TotalMinutes <= 15;
    }

    private bool CanPost()
    {
        return !string.IsNullOrWhiteSpace(Content) 
               && Content.Length <= MaxCharacters 
               && !IsPosting;
    }

    private bool CanAddImage()
    {
        return ImagePaths.Count < 4;
    }

    private async Task PostAsync()
    {
        try
        {
            IsPosting = true;
            ErrorMessage = null;

            // Check if offline - posts cannot be created offline
            if (_offlineSyncManager != null && !await _offlineSyncManager.IsOnlineAsync())
            {
                MessageBox.Show("You cannot create or edit posts while offline. Please connect to the internet and try again.",
                    "Offline", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditing && _editingPost != null)
            {
                // Update existing post
                var updateDto = new UpdatePostDto(_editingPost.Id, Content);
                await _postService.UpdatePostAsync(_currentUserId, updateDto);
                PostUpdated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Create new post
                var createDto = new CreatePostDto(
                    Content,
                    ImagePaths.Any() ? ImagePaths.ToList() : null
                );
                await _postService.CreatePostAsync(_currentUserId, createDto);
                PostCreated?.Invoke(this, EventArgs.Empty);
            }

            // Clear form
            Content = string.Empty;
            ImagePaths.Clear();
            _editingPost = null;
            OnPropertyChanged(nameof(IsEditing));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsPosting = false;
        }
    }

    private void AddImage()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
            Multiselect = true,
            Title = "Select Images"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            foreach (var fileName in openFileDialog.FileNames)
            {
                if (ImagePaths.Count >= 4)
                {
                    ErrorMessage = "You can only attach up to 4 images";
                    break;
                }

                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Length > 5 * 1024 * 1024) // 5MB
                {
                    ErrorMessage = $"Image {Path.GetFileName(fileName)} is too large. Maximum size is 5MB";
                    continue;
                }

                if (!ImagePaths.Contains(fileName))
                {
                    ImagePaths.Add(fileName);
                }
            }
        }
    }

    private void RemoveImage(object? parameter)
    {
        if (parameter is string imagePath && ImagePaths.Contains(imagePath))
        {
            ImagePaths.Remove(imagePath);
            ErrorMessage = null;
        }
    }

    private void Cancel()
    {
        Content = string.Empty;
        ImagePaths.Clear();
        _editingPost = null;
        ErrorMessage = null;
        OnPropertyChanged(nameof(IsEditing));
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
