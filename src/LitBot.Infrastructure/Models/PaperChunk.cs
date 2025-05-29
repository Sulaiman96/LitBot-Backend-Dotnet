using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("paper_chunks")]
public class PaperChunk : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("paper_url")]
    public required string PaperUrl { get; set; }

    [Column("chunk_text")]
    public required string ChunkText { get; set; }

    [Column("chunk_index")]
    public int ChunkIndex { get; set; }

    [Column("embedding_vector")]
    public float[]? EmbeddingVector { get; set; }

    [Column("token_count")]
    public int? TokenCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relationships
    [Reference(typeof(Paper))]
    public Paper? Paper { get; set; }
}