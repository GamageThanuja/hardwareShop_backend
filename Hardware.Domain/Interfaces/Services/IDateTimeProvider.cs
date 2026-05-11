namespace Hardware.Domain.Interfaces.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcOffsetNow { get; }
    DateOnly TodayUtc { get; }
}
