using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("profiles")]
public class Profile : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; }

    [Column("last_name")]
    public string? LastName { get; set; }

    [Column("profile_pic")]
    public string? ProfilePic { get; set; }

    [Column("current_daily_token")]
    public long CurrentDailyToken { get; set; } = 50000;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}