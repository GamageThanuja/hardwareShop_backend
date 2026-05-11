namespace Hardware.Application.DTOs.Users;

public sealed record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
