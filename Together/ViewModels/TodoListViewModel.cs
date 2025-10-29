using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Together.Application.DTOs;
using Together.Application.Interfaces;
using Together.Presentation.Commands;
using Together.Services;

namespace Together.Presentation.ViewModels;

public class TodoListViewModel : ViewModelBase, INavigationAware
{
    private readonly ITodoService _todoService;
    private readonly ICoupleConnectionService _connectionService;
    private Guid _currentUserId;
    private bool _isLoading;
    private bool _isCreating;
    private string _newTitle = string.Empty;
    private string? _newDescription;
    private DateTime? _newDueDate;
    private Guid? _newAssignedTo;
    private string _newTags = string.Empty;
    private string _filterTags = string.Empty;
    private bool _showOverdueOnly;
    private Guid? _partnerId;

    public TodoListViewModel(ITodoService todoService, ICoupleConnectionService connectionService)
    {
        _todoService = todoService;
        _connectionService = connectionService;

        TodoItems = new ObservableCollection<TodoItemViewModel>();

        CreateCommand = new RelayCommand(_ => StartCreate());
        SaveNewCommand = new RelayCommand(async _ => await SaveNewAsync(), _ => CanSaveNew());
        CancelNewCommand = new RelayCommand(_ => CancelCreate());
        RefreshCommand = new RelayCommand(async _ => await LoadTodoItemsAsync());
        ApplyFilterCommand = new RelayCommand(async _ => await ApplyFilterAsync());
        ClearFilterCommand = new RelayCommand(async _ => await ClearFilterAsync());
    }

    public void OnNavigatedTo(object? parameter)
    {
        // Get current user from application properties
        var currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as UserDto;
        if (currentUser != null)
        {
            _currentUserId = currentUser.Id;
            _ = InitializeAsync();
        }
    }

    public void OnNavigatedFrom()
    {
        // Cleanup if needed
    }

    public ObservableCollection<TodoItemViewModel> TodoItems { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsCreating
    {
        get => _isCreating;
        set => SetProperty(ref _isCreating, value);
    }

    public string NewTitle
    {
        get => _newTitle;
        set
        {
            SetProperty(ref _newTitle, value);
            ((RelayCommand)SaveNewCommand).RaiseCanExecuteChanged();
        }
    }

    public string? NewDescription
    {
        get => _newDescription;
        set => SetProperty(ref _newDescription, value);
    }

    public DateTime? NewDueDate
    {
        get => _newDueDate;
        set => SetProperty(ref _newDueDate, value);
    }

    public Guid? NewAssignedTo
    {
        get => _newAssignedTo;
        set => SetProperty(ref _newAssignedTo, value);
    }

    public string NewTags
    {
        get => _newTags;
        set => SetProperty(ref _newTags, value);
    }

    public string FilterTags
    {
        get => _filterTags;
        set => SetProperty(ref _filterTags, value);
    }

    public bool ShowOverdueOnly
    {
        get => _showOverdueOnly;
        set => SetProperty(ref _showOverdueOnly, value);
    }

    public Guid? PartnerId
    {
        get => _partnerId;
        set => SetProperty(ref _partnerId, value);
    }

    public Guid CurrentUserId => _currentUserId;

    public ICommand CreateCommand { get; }
    public ICommand SaveNewCommand { get; }
    public ICommand CancelNewCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ApplyFilterCommand { get; }
    public ICommand ClearFilterCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            // Get partner ID
            var connection = await _connectionService.GetUserConnectionAsync(_currentUserId);
            if (connection != null)
            {
                PartnerId = connection.User1.Id == _currentUserId ? connection.User2.Id : connection.User1.Id;
            }

            await LoadTodoItemsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing todo list: {ex.Message}");
        }
    }

    private async Task LoadTodoItemsAsync()
    {
        IsLoading = true;
        try
        {
            var todos = await _todoService.GetTodoItemsAsync(_currentUserId);
            
            TodoItems.Clear();
            foreach (var todo in todos)
            {
                var viewModel = new TodoItemViewModel(todo, _todoService, _currentUserId);
                viewModel.TodoUpdated += OnTodoUpdated;
                viewModel.TodoDeleted += OnTodoDeleted;
                TodoItems.Add(viewModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading todo items: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void StartCreate()
    {
        NewTitle = string.Empty;
        NewDescription = null;
        NewDueDate = null;
        NewAssignedTo = null;
        NewTags = string.Empty;
        IsCreating = true;
    }

    private async Task SaveNewAsync()
    {
        try
        {
            var tags = _newTags
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            var createDto = new CreateTodoItemDto(
                _newTitle,
                _newDescription,
                _newAssignedTo,
                _newDueDate,
                tags
            );

            var newTodo = await _todoService.CreateTodoItemAsync(_currentUserId, createDto);
            
            var viewModel = new TodoItemViewModel(newTodo, _todoService, _currentUserId);
            viewModel.TodoUpdated += OnTodoUpdated;
            viewModel.TodoDeleted += OnTodoDeleted;
            TodoItems.Insert(0, viewModel);

            IsCreating = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating todo: {ex.Message}");
        }
    }

    private void CancelCreate()
    {
        IsCreating = false;
    }

    private bool CanSaveNew()
    {
        return !string.IsNullOrWhiteSpace(_newTitle);
    }

    private async Task ApplyFilterAsync()
    {
        IsLoading = true;
        try
        {
            if (_showOverdueOnly)
            {
                var todos = await _todoService.GetOverdueItemsAsync(_currentUserId);
                UpdateTodoList(todos);
            }
            else if (!string.IsNullOrWhiteSpace(_filterTags))
            {
                var tags = _filterTags
                    .Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                var todos = await _todoService.GetTodoItemsByTagsAsync(_currentUserId, tags);
                UpdateTodoList(todos);
            }
            else
            {
                await LoadTodoItemsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying filter: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ClearFilterAsync()
    {
        FilterTags = string.Empty;
        ShowOverdueOnly = false;
        await LoadTodoItemsAsync();
    }

    private void UpdateTodoList(IEnumerable<TodoItemDto> todos)
    {
        TodoItems.Clear();
        foreach (var todo in todos)
        {
            var viewModel = new TodoItemViewModel(todo, _todoService, _currentUserId);
            viewModel.TodoUpdated += OnTodoUpdated;
            viewModel.TodoDeleted += OnTodoDeleted;
            TodoItems.Add(viewModel);
        }
    }

    private void OnTodoUpdated(object? sender, EventArgs e)
    {
        // Optionally refresh the list or reorder items
    }

    private void OnTodoDeleted(object? sender, EventArgs e)
    {
        if (sender is TodoItemViewModel viewModel)
        {
            TodoItems.Remove(viewModel);
        }
    }
}
