using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Extensions;

public static class ProblemDetailsExtensions
{
    public static ObjectResult ProblemResponse(int status, string title, string detail, string? instance = null) =>
        new(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = instance,
            Type = $"https://httpstatuses.io/{status}"
        }) { StatusCode = status };

    public static ObjectResult NotFound(string detail, string? instance = null) =>
        ProblemResponse(StatusCodes.Status404NotFound, "Not Found", detail, instance);

    public static ObjectResult BadRequest(string detail, string? instance = null) =>
        ProblemResponse(StatusCodes.Status400BadRequest, "Bad Request", detail, instance);

    public static ObjectResult Unauthorized(string detail = "Authentication required.", string? instance = null) =>
        ProblemResponse(StatusCodes.Status401Unauthorized, "Unauthorized", detail, instance);

    public static ObjectResult Forbidden(string detail = "Access denied.", string? instance = null) =>
        ProblemResponse(StatusCodes.Status403Forbidden, "Forbidden", detail, instance);

    public static ObjectResult Conflict(string detail, string? instance = null) =>
        ProblemResponse(StatusCodes.Status409Conflict, "Conflict", detail, instance);

    public static ObjectResult InternalServerError(string detail = "An unexpected error occurred.",
        string? instance = null) =>
        ProblemResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", detail, instance);
}
