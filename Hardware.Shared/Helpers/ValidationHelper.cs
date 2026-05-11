using System.Text.RegularExpressions;

namespace Hardware.Shared.Helpers;

public static partial class ValidationHelper
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^\+?[1-9]\d{1,14}$")]
    private static partial Regex PhoneRegex();

    public static bool IsEmail(string? value) =>
        !string.IsNullOrWhiteSpace(value) && EmailRegex().IsMatch(value);

    public static bool IsPhoneNumber(string? value) =>
        !string.IsNullOrWhiteSpace(value) && PhoneRegex().IsMatch(value);
}
