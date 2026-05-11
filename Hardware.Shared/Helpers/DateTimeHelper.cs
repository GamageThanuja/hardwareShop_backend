namespace Hardware.Shared.Helpers;

public static class DateTimeHelper
{
    public static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static int CalculateAge(DateTime dateOfBirth, DateTime? asOf = null)
    {
        var reference = asOf ?? DateTime.UtcNow;
        var age = reference.Year - dateOfBirth.Year;
        if (reference < dateOfBirth.AddYears(age)) age--;
        return age;
    }
}
