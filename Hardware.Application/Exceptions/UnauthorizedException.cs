namespace Hardware.Application.Exceptions;

public sealed class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Authentication required.")
        : base(message, "UNAUTHORIZED")
    {
    }
}
