# Dependency Injection Registration for Todo List Feature

## Services to Register

Add the following registrations to your DI container configuration (typically in `App.xaml.cs` or a startup configuration file):

```csharp
// Repository
services.AddScoped<ITodoItemRepository, TodoItemRepository>();

// Service
services.AddScoped<ITodoService, TodoService>();

// ViewModels (if using DI for ViewModels)
services.AddTransient<TodoListViewModel>();
services.AddTransient<TodoItemViewModel>();
```

## Example Registration in App.xaml.cs

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // ... existing registrations ...

    // Todo Feature
    services.AddScoped<ITodoItemRepository, TodoItemRepository>();
    services.AddScoped<ITodoService, TodoService>();
    
    // ... other registrations ...
}
```

## Dependencies

The TodoService requires:
- `ITodoItemRepository`
- `ICoupleConnectionRepository` (should already be registered)
- `IUserRepository` (should already be registered)

The TodoListViewModel requires:
- `ITodoService`
- `ICoupleConnectionService` (should already be registered)
- Current user ID (passed as constructor parameter)

The TodoItemViewModel requires:
- `TodoItemDto` (passed as constructor parameter)
- `ITodoService`
- Current user ID (passed as constructor parameter)

## Usage in Views

To use the TodoListView in your application:

```csharp
// In your navigation or view creation code
var todoService = serviceProvider.GetRequiredService<ITodoService>();
var connectionService = serviceProvider.GetRequiredService<ICoupleConnectionService>();
var currentUserId = GetCurrentUserId(); // Your method to get current user

var viewModel = new TodoListViewModel(todoService, connectionService, currentUserId);
var view = new TodoListView { DataContext = viewModel };
```

## Database Migration

Ensure the `todo_items` table exists in your database. The entity configuration is already set up in `TodoItemConfiguration.cs`. Run migrations if needed:

```bash
dotnet ef migrations add AddTodoItems
dotnet ef database update
```
