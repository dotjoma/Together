namespace Together.Application.Exceptions;

public class TogetherException : Exception
{
    public TogetherException(string message) : base(message) { }
    public TogetherException(string message, Exception inner) : base(message, inner) { }
}
