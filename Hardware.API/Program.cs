using Hangfire;
using Hardware.API.Middlewares;
using Hardware.API.Startup;
using Hardware.Infrastructure.BackgroundJobs;
using Hardware.Infrastructure.Notifications;
using Serilog;
using Serilog.Events;

ThreadPool.SetMinThreads(Environment.ProcessorCount * 32, Environment.ProcessorCount * 32);
ThreadPool.SetMaxThreads(Environment.ProcessorCount * 200, Environment.ProcessorCount * 200);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxConcurrentConnections = 1000;
    o.Limits.MaxRequestBodySize = 25 * 1024 * 1024;
    o.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
});

builder.AddServiceDefaults();

builder.Host.UseSerilog((ctx, sp, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(sp)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File("Logs/hw-.log",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 50_000_000,
            retainedFileCountLimit: 14);

    var connectionString = ctx.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
        cfg.WriteTo.PostgreSQL(
            connectionString,
            "Logs",
            needAutoCreateTable: true,
            restrictedToMinimumLevel: LogEventLevel.Warning,
            useCopy: true,
            schemaName: "public");
});

builder.Services.RegisterServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue("AppSettings:EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hardware API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseResponseCompression();
app.UseRouting();

app.UseCors("Default");
app.UseRateLimiter();
app.UseOutputCache();

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/hangfire"))
    {
        var queryToken = ctx.Request.Query["access_token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryToken))
            ctx.Response.Cookies.Append("hw_hangfire_jwt", queryToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = ctx.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/hangfire",
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Hardware Background Jobs",
    Authorization = [new HangfireAuthorizationFilter()]
});

app.MapDefaultEndpoints();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

HangfireJobConfiguration.ConfigureRecurringJobs(app.Services);

if (app.Environment.IsDevelopment()) await app.ApplyMigrationsAsync();

try
{
    Log.Information("Starting Hardware API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Hardware API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
