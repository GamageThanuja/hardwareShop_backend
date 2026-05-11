using System.Security.Cryptography;
using System.Text;

namespace Hardware.Infrastructure.Identity;

public static class RefreshTokenHasher
{
    public static (string Raw, string Hash) Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var raw = Convert.ToBase64String(bytes);
        return (raw, Hash(raw));
    }

    public static string Hash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}
