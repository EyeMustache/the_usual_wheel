namespace TheUsualWheelProject.Models;

using Dapper.Contrib.Extensions;

// This will make use of .json maybe? I want to ask Gemini to generate breaks for the movies and use that in future.
// Either way this is a placeholder for now, manual or automatic will be implemented in the future.
[Table("MovieBreak")]
public class Break : IModel
{
    [Key]
    public int Id { get; set; }
    public int MovieId { get; set; } // Foreign key to Movie, get details from there
    public required int BreakNumber { get; set; }
    public required string Timestamp { get; set; }
    public required string VibeCheck { get; set; }
    public required string Recap { get; set; }
}