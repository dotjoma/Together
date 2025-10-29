using Together.Application.Exceptions;

namespace Together.Application.Common;

/// <summary>
/// Maps technical exceptions to user-friendly error messages
/// </summary>
public static class ErrorMessageMapper
{
    private static readonly Dictionary<Type, string> _defaultMessages = new()
    {
        { typeof(AuthenticationException), "Authentication failed. Please check your credentials and try again." },
        { typeof(ValidationException), "The information you provided is invalid. Please check the form and try again." },
        { typeof(NotFoundException), "The requested item could not be found." },
        { typeof(BusinessRuleViolationException), "This action cannot be completed due to business rules." },
        { typeof(TogetherException), "An error occurred while processing your request." }
    };

    /// <summary>
    /// Gets a user-friendly error message for the given exception
    /// </summary>
    public static string GetUserFriendlyMessage(Exception exception)
    {
        if (exception == null)
            return "An unexpected error occurred.";

        // Use the exception's message if it's already user-friendly
        if (exception is TogetherException togetherException)
        {
            // For specific exceptions, use their message if it's descriptive
            if (exception is AuthenticationException || 
                exception is NotFoundException || 
                exception is BusinessRuleViolationException)
            {
                return exception.Message;
            }

            // For ValidationException, return a generic message (details are in Errors property)
            if (exception is ValidationException)
            {
                return "Please correct the validation errors and try again.";
            }
        }

        // Check if we have a default message for this exception type
        var exceptionType = exception.GetType();
        if (_defaultMessages.TryGetValue(exceptionType, out var message))
        {
            return message;
        }

        // Check base types
        foreach (var kvp in _defaultMessages)
        {
            if (kvp.Key.IsAssignableFrom(exceptionType))
            {
                return kvp.Value;
            }
        }

        // Fallback for unknown exceptions
        return "An unexpected error occurred. Please try again later.";
    }

    /// <summary>
    /// Gets a detailed error message for logging purposes
    /// </summary>
    public static string GetDetailedMessage(Exception exception)
    {
        if (exception == null)
            return "Unknown error";

        var message = $"{exception.GetType().Name}: {exception.Message}";
        
        if (exception.InnerException != null)
        {
            message += $" | Inner: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}";
        }

        return message;
    }
}
