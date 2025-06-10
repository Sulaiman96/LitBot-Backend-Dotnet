namespace LitBot.Core.DTOs.Auth;

public class ResetPasswordRequestDto
{
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}