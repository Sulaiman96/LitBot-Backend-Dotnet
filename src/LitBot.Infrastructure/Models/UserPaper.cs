using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace LitBot.Infrastructure.Models;

[Table("user_papers")]
public class UserPaper : BaseModel
{
    [PrimaryKey("user_id")]
    public Guid UserId { get; set; }

    [PrimaryKey("paper_url")]
    public required string PaperUrl { get; set; }

    [Column("is_favourite")]
    public bool IsFavourite { get; set; } = false;

    [Column("upload_date")]
    public DateTime UploadDate { get; set; }

    // Relationships
    [Reference(typeof(Paper))]
    public Paper? Paper { get; set; }

    [Reference(typeof(Profile))]
    public Profile? Owner { get; set; }
}