using TheUsualWheelProject.APIKeys;
using TheUsualWheelProject.Models;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.Movies;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public class TmdbService : LoggingBase<TmdbService>
{
    private readonly TMDbClient _tmdbClient;
    public TmdbService(ILogger<TmdbService> logger) : base(logger)
    {
        _tmdbClient = new TMDbClient(TmdbApiKey.APIKEY);
    }

    public async Task<List<SearchMovie>?> SearchMoviesAsync(string title, int page = 1)
    {
        Logger.LogInformation("Searching for movies with title:{title} on page {page}.", title, page);
        
        // Pass the page to the TMDbClient
        var searchResults = await _tmdbClient.SearchMovieAsync(title, page: page);
        if (searchResults?.Results == null) 
            return null;

        return searchResults.Results.ToList();
    }

    public async Task<SingleResultContainer<Dictionary<string, WatchProviders>>?> GetMovieWatchProvidersAsync(int tmdbId)
    {
        try
        {
            Logger.LogInformation("Fetching watch providers from TMDb for ID {tmdbId}.", tmdbId);
            return await _tmdbClient.GetMovieWatchProvidersAsync(tmdbId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching watch providers for TMDb ID {tmdbId}.", tmdbId);
            return null;
        }
    }

    public async Task<Models.Movie?> GetMovieDetailsAsync(int tmdbId)
    {
        try
        {
            Logger.LogInformation("Fetching movie details from TMDb for ID {tmdbId}.", tmdbId);
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
            // Debug output
            Logger.LogInformation("Constructed movie details for TMDb ID {tmdbId}.", tmdbId);

            Models.Movie constructedMovie = new()
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

            Logger.LogInformation("Constructed movie details for TMDb ID {tmdbId}: {@movie}", tmdbId, constructedMovie);

            return constructedMovie;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching movie details from TMDb for ID {tmdbId}.", tmdbId);
            return null;
        }
    }

}