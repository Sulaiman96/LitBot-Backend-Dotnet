using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("chat_messages")]
public class ChatMessage : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("conversation_id")]
    public long ConversationId { get; set; }

    [Column("role")]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("token_cost")]
    public long? TokenCost { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relationships
    [Reference(typeof(ChatConversation))]
    public ChatConversation? Conversation { get; set; }
}