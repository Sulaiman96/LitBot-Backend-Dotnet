using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("chat_conversations")]
public class ChatConversation : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("paper_url")]
    public string? PaperUrl { get; set; }

    [Column("summary_id")]
    public long? SummaryId { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Relationships
    [Reference(typeof(Profile))]
    public Profile? User { get; set; }

    [Reference(typeof(Paper))]
    public Paper? Paper { get; set; }

    [Reference(typeof(Summary))]
    public Summary? Summary { get; set; }

    [Reference(typeof(ChatMessage), useInnerJoin: false)]
    public List<ChatMessage> Messages { get; set; } = [];
}