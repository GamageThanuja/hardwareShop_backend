using Hardware.Domain.Interfaces.Services;

namespace Hardware.Infrastructure.Identity;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset UtcOffsetNow => DateTimeOffset.UtcNow;
    public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
}
