namespace LitBot.Core.DTOs.Auth;

public class ChangePasswordRequestDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}