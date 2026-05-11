using Hardware.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Hardware.Infrastructure.Identity;

public static class DeviceInfoExtensions
{
    public static DeviceInfo ToDeviceInfo(this HttpContext? context)
    {
        if (context is null) return DeviceInfo.Unknown;

        var headers = context.Request.Headers;
        return new DeviceInfo(
            NullIfEmpty(headers["X-Device-Id"].ToString()),
            NullIfEmpty(headers["X-Device-Name"].ToString()),
            NullIfEmpty(headers.UserAgent.ToString()),
            context.Connection.RemoteIpAddress?.ToString(),
            NullIfEmpty(headers["X-Client-Type"].ToString()));
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
