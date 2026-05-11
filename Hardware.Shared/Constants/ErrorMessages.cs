namespace Hardware.Shared.Constants;

public static class ErrorMessages
{
    public const string Unauthorized = "Authentication required.";
    public const string Forbidden = "You do not have permission to perform this action.";
    public const string NotFound = "The requested resource was not found.";
    public const string ValidationFailed = "One or more validation errors occurred.";
    public const string Conflict = "The request conflicts with the current state.";
    public const string InternalServerError = "An unexpected error occurred. Please try again later.";
    public const string InvalidCredentials = "Invalid username or password.";
    public const string TokenExpired = "The token has expired.";
    public const string TokenInvalid = "The token is invalid.";
}
