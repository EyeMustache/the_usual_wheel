namespace TheUsualWheelProject.Models;
// Used for Tmdb movies that we fetch from the API
public class TmdbMovie
{
    public int TmdbId { get; set; }
    public required string Title { get; set; }
    public int Year { get; set; }
    public required string Genre { get; set; }
    public required string Director { get; set; }
    public required string Cast { get; set; }
    public int DurationInMinutes { get; set; }
    public string? Synopsis { get; set; }
    public string? PosterUrl { get; set; }
    public double TmdbRating { get; set; }

    public override string ToString()
    {
        return $"{Title} ({Year}) - Directed by {Director} - Duration: {DurationInMinutes} mins - Genre: {Genre}"; 
    }

    public Movie MapToDbMovie()
    {
        Movie movie = new ()
        {
            TmdbId = this.TmdbId,
            Title = this.Title,
            Year = this.Year,
            Genre = this.Genre,
            Director = this.Director,
            Cast = this.Cast,
            DurationInMinutes = this.DurationInMinutes,
            Synopsis = this.Synopsis,
            PosterUrl = this.PosterUrl,
            TmdbRating = this.TmdbRating
        };
        return movie;
    }
}