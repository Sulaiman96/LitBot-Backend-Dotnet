using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("token_usage")]
public class TokenUsage : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("operation_type")]
    public string OperationType { get; set; } = string.Empty; // "summary", "chat", "embedding"

    [Column("tokens_used")]
    public long TokensUsed { get; set; }

    [Column("cost")]
    public decimal? Cost { get; set; }

    [Column("related_entity_id")]
    public string? RelatedEntityId { get; set; } // Can be summary_id, conversation_id, etc.

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relationships
    [Reference(typeof(Profile))]
    public Profile? User { get; set; }
}