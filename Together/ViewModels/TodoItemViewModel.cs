using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;

namespace Together.Presentation.ViewModels;

public class TodoItemViewModel : ViewModelBase
{
    private readonly ITodoService _todoService;
    private readonly Guid _currentUserId;
    private TodoItemDto _todoItem;
    private bool _isEditing;
    private string _editTitle;
    private string? _editDescription;
    private DateTime? _editDueDate;
    private Guid? _editAssignedTo;
    private string _editTags;

    public TodoItemViewModel(TodoItemDto todoItem, ITodoService todoService, Guid currentUserId)
    {
        _todoItem = todoItem;
        _todoService = todoService;
        _currentUserId = currentUserId;
        _editTitle = todoItem.Title;
        _editDescription = todoItem.Description;
        _editDueDate = todoItem.DueDate;
        _editAssignedTo = todoItem.AssignedTo;
        _editTags = string.Join(", ", todoItem.Tags);

        ToggleCompleteCommand = new RelayCommand(async _ => await ToggleCompleteAsync());
        EditCommand = new RelayCommand(_ => StartEdit());
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => CancelEdit());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync());
    }

    public Guid Id => _todoItem.Id;
    public string Title => _todoItem.Title;
    public string? Description => _todoItem.Description;
    public Guid? AssignedTo => _todoItem.AssignedTo;
    public string? AssignedToUsername => _todoItem.AssignedToUsername;
    public string CreatedByUsername => _todoItem.CreatedByUsername;
    public DateTime? DueDate => _todoItem.DueDate;
    public bool Completed => _todoItem.Completed;
    public DateTime? CompletedAt => _todoItem.CompletedAt;
    public List<string> Tags => _todoItem.Tags;
    public bool IsOverdue => _todoItem.IsOverdue;
    public DateTime CreatedAt => _todoItem.CreatedAt;

    public string DisplayDueDate => DueDate.HasValue ? DueDate.Value.ToString("MMM dd, yyyy") : "No due date";
    public string DisplayAssignment => AssignedToUsername ?? "Unassigned";
    public string DisplayTags => Tags.Count > 0 ? string.Join(", ", Tags) : "No tags";

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public string EditTitle
    {
        get => _editTitle;
        set
        {
            SetProperty(ref _editTitle, value);
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string? EditDescription
    {
        get => _editDescription;
        set => SetProperty(ref _editDescription, value);
    }

    public DateTime? EditDueDate
    {
        get => _editDueDate;
        set => SetProperty(ref _editDueDate, value);
    }

    public Guid? EditAssignedTo
    {
        get => _editAssignedTo;
        set => SetProperty(ref _editAssignedTo, value);
    }

    public string EditTags
    {
        get => _editTags;
        set => SetProperty(ref _editTags, value);
    }

    public ICommand ToggleCompleteCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    public event EventHandler? TodoUpdated;
    public event EventHandler? TodoDeleted;

    private async Task ToggleCompleteAsync()
    {
        try
        {
            TodoItemDto updatedTodo;
            if (Completed)
            {
                updatedTodo = await _todoService.MarkAsIncompleteAsync(Id, _currentUserId);
            }
            else
            {
                updatedTodo = await _todoService.MarkAsCompleteAsync(Id, _currentUserId);
            }

            UpdateFromDto(updatedTodo);
            TodoUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error toggling todo completion: {ex.Message}");
        }
    }

    private void StartEdit()
    {
        _editTitle = Title;
        _editDescription = Description;
        _editDueDate = DueDate;
        _editAssignedTo = AssignedTo;
        _editTags = string.Join(", ", Tags);
        IsEditing = true;
    }

    private async Task SaveAsync()
    {
        try
        {
            var tags = _editTags
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            var updateDto = new UpdateTodoItemDto(
                _editTitle,
                _editDescription,
                _editAssignedTo,
                _editDueDate,
                tags
            );

            var updatedTodo = await _todoService.UpdateTodoItemAsync(Id, _currentUserId, updateDto);
            UpdateFromDto(updatedTodo);
            IsEditing = false;
            TodoUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error updating todo: {ex.Message}");
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
    }

    private async Task DeleteAsync()
    {
        try
        {
            await _todoService.DeleteTodoItemAsync(Id, _currentUserId);
            TodoDeleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            // TODO: Show error message to user
            System.Diagnostics.Debug.WriteLine($"Error deleting todo: {ex.Message}");
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(_editTitle);
    }

    private void UpdateFromDto(TodoItemDto dto)
    {
        _todoItem = dto;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(AssignedTo));
        OnPropertyChanged(nameof(AssignedToUsername));
        OnPropertyChanged(nameof(DueDate));
        OnPropertyChanged(nameof(Completed));
        OnPropertyChanged(nameof(CompletedAt));
        OnPropertyChanged(nameof(Tags));
        OnPropertyChanged(nameof(IsOverdue));
        OnPropertyChanged(nameof(DisplayDueDate));
        OnPropertyChanged(nameof(DisplayAssignment));
        OnPropertyChanged(nameof(DisplayTags));
    }
}
