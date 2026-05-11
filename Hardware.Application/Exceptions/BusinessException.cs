namespace Hardware.Application.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string? ErrorCode { get; }
}
