namespace LitBot.Core.DTOs.Auth;

public class UserResponseDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}