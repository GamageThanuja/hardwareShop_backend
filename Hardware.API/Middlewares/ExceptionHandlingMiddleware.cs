using System.Net;
using System.Text.Json;
using Hardware.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problem) = MapException(exception, context);

        logger.LogError(exception,
            "Unhandled exception {ExceptionType} on {Method} {Path} (CorrelationId={CorrelationId})",
            exception.GetType().Name,
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private (HttpStatusCode StatusCode, ProblemDetails Problem) MapException(Exception exception, HttpContext context)
    {
        return exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, BuildValidation(ve, context)),
            NotFoundException => (HttpStatusCode.NotFound,
                Build(HttpStatusCode.NotFound, "Not Found", exception.Message, context)),
            UnauthorizedException => (HttpStatusCode.Unauthorized,
                Build(HttpStatusCode.Unauthorized, "Unauthorized", exception.Message, context)),
            ForbiddenException => (HttpStatusCode.Forbidden,
                Build(HttpStatusCode.Forbidden, "Forbidden", exception.Message, context)),
            ConflictException => (HttpStatusCode.Conflict,
                Build(HttpStatusCode.Conflict, "Conflict", exception.Message, context)),
            BusinessException be => (HttpStatusCode.BadRequest,
                Build(HttpStatusCode.BadRequest, "Bad Request", be.Message, context)),
            _ => (HttpStatusCode.InternalServerError, Build(
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.",
                context))
        };
    }

    private static ProblemDetails Build(HttpStatusCode status, string title, string detail, HttpContext context) =>
        new()
        {
            Status = (int)status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.io/{(int)status}",
            Extensions = { ["correlationId"] = context.TraceIdentifier }
        };

    private static ValidationProblemDetails BuildValidation(ValidationException ex, HttpContext context) =>
        new(ex.Errors)
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Failed",
            Detail = ex.Message,
            Instance = context.Request.Path,
            Type = "https://httpstatuses.io/400",
            Extensions = { ["correlationId"] = context.TraceIdentifier }
        };
}
