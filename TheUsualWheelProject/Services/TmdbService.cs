using TheUsualWheelProject.APIKeys;
using TheUsualWheelProject.Models;
using TMDbLib.Client;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.Movies;

namespace TheUsualWheelProject.Services;

public class TmdbService
{
    private readonly TMDbClient _tmdbClient;
    public TmdbService()
    {
        _tmdbClient = new TMDbClient(TmdbApiKey.APIKEY);
    }

    public async Task<List<SearchMovie>?> SearchMoviesAsync(string title)
    {
        var searchResults = await _tmdbClient.SearchMovieAsync(title);
        if (searchResults?.Results == null) 
            return null;

        return searchResults.Results.ToList();
    }

    public async Task<Models.Movie?> GetMovieDetailsAsync(int tmdbId)
    {
        try
        {
            var tmdbMovie = await _tmdbClient.GetMovieAsync(tmdbId, MovieMethods.Credits);
            if (tmdbMovie == null)
                return null;

            // Join all genres as a comma-separated string
            string genreString = tmdbMovie.Genres != null && tmdbMovie.Genres.Any()
                ? string.Join(", ", tmdbMovie.Genres.Select(g => g.Name))
                : "Unknown Genre";

            // Get the director's name from the crew list
            string director = tmdbMovie.Credits?.Crew != null
                ? tmdbMovie.Credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name ?? "Unknown Director"
                : "Unknown Director";

            // Construct poster URL if PosterPath is available
            string? posterUrl = tmdbMovie.PosterPath != null
                ? $"https://image.tmdb.org/t/p/w500{tmdbMovie.PosterPath}"
                : null;

            // Join top 5 cast members as a comma-separated string
            string castString = tmdbMovie.Credits?.Cast != null && tmdbMovie.Credits.Cast.Any()
                ? string.Join(", ", tmdbMovie.Credits.Cast.Take(5).Select(c => c.Name))
                : "Unknown Cast";
 
            return new Models.Movie
            {
                Title = tmdbMovie.Title ?? "Unknown Title",
                Year = tmdbMovie.ReleaseDate?.Year ?? 0,
                Genre = genreString,
                Director = director,
                Cast = castString,
                DurationInMinutes = tmdbMovie.Runtime ?? 0,
                Synopsis = tmdbMovie.Overview ?? "Unknown Synopsis",
                PosterUrl = posterUrl,
                TmdbRating = tmdbMovie.VoteAverage,
                LetterboxdRating = null, // Letterboxd rating is not available from TMDb
                TmdbId = tmdbMovie.Id
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching movie details from TMDb: {ex.Message}");
            return null;
        }
    }

}