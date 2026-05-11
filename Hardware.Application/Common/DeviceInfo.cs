namespace Hardware.Application.Common;

public sealed record DeviceInfo(
    string? DeviceId,
    string? DeviceName,
    string? UserAgent,
    string? IpAddress,
    string? ClientType)
{
    public static DeviceInfo Unknown => new(null, null, null, null, null);
}
