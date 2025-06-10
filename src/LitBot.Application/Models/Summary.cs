using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("summaries")]
public class Summary : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("paper_url")]
    public required string PaperUrl { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("raw_summary")]
    public string? RawSummary { get; set; }

    [Column("raw_summary_token_cost")]
    public long? RawSummaryTokenCost { get; set; }

    [Column("embedded_summary")]
    public string? EmbeddedSummary { get; set; }

    [Column("embedded_summary_token_cost")]
    public long? EmbeddedSummaryTokenCost { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relationships
    [Reference(typeof(Paper))]
    public Paper? Paper { get; set; }

    [Reference(typeof(Profile))]
    public Profile? Author { get; set; }

    [Reference(typeof(Feedback), useInnerJoin: false)]
    public List<Feedback> Feedbacks { get; set; } = [];

    [Reference(typeof(ChatConversation), useInnerJoin: false)]
    public List<ChatConversation> ChatConversations { get; set; } = [];
}