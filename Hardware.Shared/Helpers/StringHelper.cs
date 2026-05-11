using System.Security.Cryptography;
using System.Text;

namespace Hardware.Shared.Helpers;

public static class StringHelper
{
    public static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength
            ? value
            : value[..maxLength];

    public static string GenerateRandomToken(int byteLength = 64) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteLength));

    public static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input.Trim().ToLowerInvariant())
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (ch is ' ' or '-' or '_') sb.Append('-');

        return sb.ToString();
    }
}
