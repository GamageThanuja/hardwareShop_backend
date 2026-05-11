namespace Hardware.Shared.Constants;

public static class CacheKeys
{
    public const string Prefix = "Hardware:";

    public static string User(Guid id) => $"{Prefix}user:{id}";
    public static string UserByEmail(string email) => $"{Prefix}user:email:{email.ToLowerInvariant()}";
    public static string UserRoles(Guid id) => $"{Prefix}user:{id}:roles";
}
