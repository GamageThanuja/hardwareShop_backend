namespace Hardware.Application.Common;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Success(T data, string message = "OK") =>
        new() { IsSuccess = true, Data = data, Message = message };

    public static ApiResponse<T> Failure(string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}

public static class ApiResponse
{
    public static ApiResponse<object> Success(string message = "OK") =>
        new() { IsSuccess = true, Message = message };

    public static ApiResponse<object> Failure(string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}
