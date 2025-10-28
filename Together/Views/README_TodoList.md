# Shared To-Do List Implementation

## Overview
The shared to-do list feature allows couples to create, manage, and track tasks together. Tasks can be assigned to either partner or both, have due dates, and be categorized with tags.

## Components

### Domain Layer
- **TodoItem Entity**: Core entity with title, description, assignment, due date, tags, and completion status
- **ITodoItemRepository**: Repository interface for data access

### Application Layer
- **TodoService**: Business logic for creating, updating, completing, and deleting todo items
- **DTOs**: TodoItemDto, CreateTodoItemDto, UpdateTodoItemDto

### Infrastructure Layer
- **TodoItemRepository**: EF Core implementation of ITodoItemRepository with filtering and sorting

### Presentation Layer
- **TodoListViewModel**: Main view model managing the list of todos, filtering, and creation
- **TodoItemViewModel**: Individual todo item view model with edit and completion functionality
- **TodoListView**: XAML view displaying the todo list with Material Design styling

## Features Implemented

### Task Management
- ✅ Create new tasks with title, description, due date, assignment, and tags
- ✅ Edit existing tasks
- ✅ Mark tasks as complete/incomplete
- ✅ Delete tasks
- ✅ Overdue detection with visual indicators

### Assignment System
- ✅ Assign tasks to either partner or leave unassigned
- ✅ Display assigned user's username
- ✅ Validation to ensure only partners in the connection can be assigned

### Tag System
- ✅ Add multiple tags to tasks (comma-separated)
- ✅ Filter tasks by tags
- ✅ Display tags on task cards

### Filtering
- ✅ Filter by tags
- ✅ Show overdue tasks only
- ✅ Clear filters to show all tasks

### UI Features
- ✅ Material Design styling
- ✅ Inline editing mode
- ✅ Completion checkboxes
- ✅ Visual indicators for overdue tasks (red text)
- ✅ Strikethrough for completed tasks
- ✅ Responsive layout

## Usage

### Creating a Task
1. Click "Add Task" button
2. Fill in the title (required), description, due date, assignment, and tags
3. Click "Save" to create the task

### Editing a Task
1. Click the pencil icon on a task card
2. Modify the fields as needed
3. Click "Save" to update or "Cancel" to discard changes

### Completing a Task
- Click the checkbox next to the task to mark it as complete
- Click again to mark as incomplete

### Filtering Tasks
1. Enter tags in the filter box (comma-separated)
2. Check "Overdue Only" to show only overdue tasks
3. Click "Apply" to filter
4. Click "Clear" to reset filters

## Requirements Satisfied

- **Requirement 5.1**: Create todo items with title, due date, and assignment ✅
- **Requirement 5.2**: Mark as complete with partner notification (notification pending real-time service) ✅
- **Requirement 5.3**: Update todo items with editing functionality ✅
- **Requirement 5.4**: Overdue detection and highlighting ✅
- **Requirement 5.5**: Tag support for categorization ✅

## Future Enhancements

### Real-time Notifications
When the real-time sync service is implemented, add:
- Partner notification when a task is created
- Partner notification when a task is completed
- Real-time updates when partner modifies tasks

### Additional Features
- Recurring tasks
- Task priority levels
- Subtasks/checklists
- File attachments
- Task comments
- Activity history

## Dependencies

### Services Required
- ITodoService
- ICoupleConnectionService

### Converters Used
- BooleanToVisibilityConverter
- InverseBooleanConverter
- BoolToStrikethroughConverter
- BoolToErrorBrushConverter
- StringToVisibilityConverter
- ZeroToVisibilityConverter

## Notes

- Tasks are scoped to couple connections - users must have an active connection to create tasks
- Only partners in the connection can be assigned to tasks
- Overdue tasks are highlighted in red
- Completed tasks show strikethrough text
- The list is sorted by completion status, then due date, then creation date
