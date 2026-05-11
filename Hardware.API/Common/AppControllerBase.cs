using System.Diagnostics;
using System.Security.Claims;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Hardware.API.Common;

public abstract class AppControllerBase(ILogger logger) : ControllerBase
{
    protected ILogger Logger { get; } = logger;

    protected Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    protected string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);

    protected string? GetUserName() => User.FindFirstValue(ClaimTypes.Name);

    protected bool IsAdmin() => User.IsInRole(RoleConstants.Admin);

    protected bool UserIsInRole(string role) => User.IsInRole(role);

    protected static Activity? StartActivity(string name) =>
        Activity.Current?.Source.StartActivity(name);

    protected static void SetActivityTag(string key, object? value) =>
        Activity.Current?.SetTag(key, value);

    protected static void SetActivityStatus(ActivityStatusCode status, string? description = null) =>
        Activity.Current?.SetStatus(status, description);
}
