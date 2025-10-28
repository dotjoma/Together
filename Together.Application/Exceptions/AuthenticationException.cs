namespace Together.Application.Exceptions;

public class AuthenticationException : TogetherException
{
    public AuthenticationException(string message) : base(message) { }
}
