namespace Hardware.Shared.Configuration;

public sealed record EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "Dhanu";
}
