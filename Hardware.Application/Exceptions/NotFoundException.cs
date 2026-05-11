namespace Hardware.Application.Exceptions;

public sealed class NotFoundException : BusinessException
{
    public NotFoundException(string resourceName, object? resourceId)
        : base($"{resourceName} with id '{resourceId}' was not found.", "NOT_FOUND")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public string ResourceName { get; }
    public object? ResourceId { get; }
}
