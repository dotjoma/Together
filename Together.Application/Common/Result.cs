namespace Together.Application.Common;

/// <summary>
/// Represents the result of an operation with success/failure state and optional data
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, Dictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors;
    }

    public static Result<T> Success(T data) => new Result<T>(true, data, null, null);

    public static Result<T> Failure(string error) => new Result<T>(false, default, error, null);

    public static Result<T> ValidationFailure(Dictionary<string, string[]> errors)
        => new Result<T>(false, default, "Validation failed", errors);
}

/// <summary>
/// Represents the result of an operation without return data
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    private Result(bool isSuccess, string? errorMessage, Dictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors;
    }

    public static Result Success() => new Result(true, null, null);

    public static Result Failure(string error) => new Result(false, error, null);

    public static Result ValidationFailure(Dictionary<string, string[]> errors)
        => new Result(false, "Validation failed", errors);
}
