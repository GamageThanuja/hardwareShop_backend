namespace Hardware.Application.Exceptions;

public sealed class ValidationException : BusinessException
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_FAILED")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }

    public IDictionary<string, string[]> Errors { get; }
}
