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

    // Relationships
    [Reference(typeof(UserPaper), useInnerJoin: false)]
    public List<UserPaper> Papers { get; set; } = [];

    [Reference(typeof(Summary), useInnerJoin: false)]
    public List<Summary> Summaries { get; set; } = [];

    [Reference(typeof(Feedback), useInnerJoin: false)]
    public List<Feedback> Feedbacks { get; set; } = [];

    [Reference(typeof(ChatConversation), useInnerJoin: false)]
    public List<ChatConversation> ChatConversations { get; set; } = [];
}