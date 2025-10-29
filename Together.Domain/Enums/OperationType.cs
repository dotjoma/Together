namespace Together.Domain.Enums;

/// <summary>
/// Types of operations that can be queued for offline sync
/// </summary>
public enum OperationType
{
    CreateJournalEntry,
    CreateMoodEntry,
    CreateTodoItem,
    UpdateTodoItem,
    CompleteTodoItem,
    CreatePost,
    LikePost,
    CreateComment
}
