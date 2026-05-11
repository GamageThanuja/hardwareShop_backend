using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hardware.Shared.Constants;

namespace Hardware.Infrastructure.BackgroundJobs;

public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
               && httpContext.User.IsInRole(RoleConstants.Admin);
    }
}
