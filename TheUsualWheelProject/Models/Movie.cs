using Dapper.Contrib.Extensions;

namespace TheUsualWheelProject.Models;

[Table("Movie")]
public class Movie : IModel
{
    [Key]
    public int Id { get; set; }
    public required string Title { get; set; }
    public int Year { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Cast { get; set; }
    public int DurationInMinutes { get; set; }
    public string? Synopsis { get; set; }
    public string? PosterUrl { get; set; }
    public double TmdbRating { get; set; }
    public double? LetterboxdRating { get; set; }
    public int TmdbId { get; set; }
    [Computed]
    public IEnumerable<WatchProvider>? WatchProviders { get; set; }
}