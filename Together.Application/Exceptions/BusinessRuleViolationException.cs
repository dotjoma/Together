namespace Together.Application.Exceptions;

public class BusinessRuleViolationException : TogetherException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
