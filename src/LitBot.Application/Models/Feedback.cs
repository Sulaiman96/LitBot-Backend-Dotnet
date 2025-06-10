using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("feedbacks")]
public class Feedback : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("paper_url")]
    public required string PaperUrl { get; set; }

    [Column("summary_id")]
    public long SummaryId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("is_helpful")]
    public bool IsHelpful { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Relationships
    [Reference(typeof(Paper))]
    public Paper? Paper { get; set; }

    [Reference(typeof(Summary))]
    public Summary? Summary { get; set; }

    [Reference(typeof(Profile))]
    public Profile? Author { get; set; }
}