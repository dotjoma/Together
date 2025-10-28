namespace Together.Application.Exceptions;

public class NotFoundException : TogetherException
{
    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key {key} not found")
    {
    }
}
