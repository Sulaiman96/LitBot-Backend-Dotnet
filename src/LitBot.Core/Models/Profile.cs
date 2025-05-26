namespace LitBot.Core.Models;

public class Profile
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ProfilePic { get; set; }
    public int CurrentDailyToken { get; set; } = 50000;
}