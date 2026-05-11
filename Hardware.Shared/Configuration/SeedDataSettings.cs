using System.ComponentModel.DataAnnotations;

namespace Hardware.Shared.Configuration;

public sealed record SeedDataSettings
{
    public const string SectionName = "SeedData";

    public bool EnableSeeding { get; init; } = true;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string AdminEmail { get; init; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string AdminUserName { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(200)]
    public string AdminPassword { get; init; } = string.Empty;

    [Required] [MaxLength(100)] public string AdminFirstName { get; init; } = string.Empty;

    [Required] [MaxLength(100)] public string AdminLastName { get; init; } = string.Empty;

    public string? AdminPhoneNumber { get; init; }
}
