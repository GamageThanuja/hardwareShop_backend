namespace Hardware.Application.Exceptions;

public sealed class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, "FORBIDDEN")
    {
    }
}
