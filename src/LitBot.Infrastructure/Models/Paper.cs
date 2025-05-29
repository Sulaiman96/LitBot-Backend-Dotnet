using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("papers")]
public class Paper : BaseModel
{
    [PrimaryKey("url")]
    public required string Url { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("extracted_text")]
    public string? ExtractedText { get; set; }

    [Column("image_captions")]
    public string? ImageCaptions { get; set; }

    [Column("is_embedded")]
    public bool IsEmbedded { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Relationships
    [Reference(typeof(UserPaper), useInnerJoin: false)]
    public List<UserPaper> Owners { get; set; } = [];

    [Reference(typeof(Summary), useInnerJoin: false)]
    public List<Summary> Summaries { get; set; } = [];

    [Reference(typeof(Feedback), useInnerJoin: false)]
    public List<Feedback> Feedbacks { get; set; } = [];

    [Reference(typeof(PaperChunk), useInnerJoin: false)]
    public List<PaperChunk> Chunks { get; set; } = [];

    [Reference(typeof(ChatConversation), useInnerJoin: false)]
    public List<ChatConversation> ChatConversations { get; set; } = [];
}