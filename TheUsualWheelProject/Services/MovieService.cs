using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public class MovieService : LoggingBase<MovieService>
{
    private readonly IMovieRepository _movieRepository;
    private readonly TmdbService _tmdbService;
    private readonly WatchProviderService _watchProviderService;

    public MovieService(IMovieRepository movieRepository, TmdbService tmdbService, WatchProviderService watchProviderService, ILogger<MovieService> logger) : base(logger)
    {
        _movieRepository = movieRepository;
        _tmdbService = tmdbService;
        _watchProviderService = watchProviderService;
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        Logger.LogInformation("Fetching all movies.");
        return await _movieRepository.GetAll();
    }

    public async Task<Movie?> GetMovieByIdAsync(int id)
    {
        Logger.LogInformation("Fetching movie by ID:{id}.", id);
        return await _movieRepository.GetById(id);
    }

    public async Task<Movie?> GetMovieByTmdbIdAsync(int tmdbId)
    {
        Logger.LogInformation("Fetching movie by TMDb ID:{tmdbId}.", tmdbId);
        return await _movieRepository.GetByTmdbId(tmdbId);
    }

    public async Task AddMovieAsync(Movie movie)
    {
        Logger.LogInformation("Adding new movie:{movie.Title} directed by {movie.Director} ({movie.Year}).", movie.Title, movie.Director, movie.Year);
        if (await MovieExistsAsync(movie.Title, movie.Director, movie.Year, movie.TmdbId))
            throw new InvalidOperationException("Movie already exists.");

        await _movieRepository.Insert(movie);
    }

    public async Task UpdateMovieAsync(Movie movie)
    {
        Logger.LogInformation("Updating movie with ID {movie.Id}.", movie.Id);
        var existingMovie = await _movieRepository.GetById(movie.Id);
        if (existingMovie == null)
            throw new InvalidOperationException("Movie does not exist.");

        if (movie.Id != existingMovie.Id)
            throw new InvalidOperationException("Movie ID mismatch.");

        await _movieRepository.Update(movie);
    }

    public async Task DeleteMovieAsync(int id)
    {
        Logger.LogInformation("Deleting movie with ID {id}.", id);
        var movie = await _movieRepository.GetById(id);
        if (movie == null)
            throw new InvalidOperationException("Movie does not exist.");

        await _movieRepository.Delete(id);
    }

    public async Task<IEnumerable<Movie>> GetMoviesByWheelAsync(int wheelId)
    {
        Logger.LogInformation("Fetching movies for wheel ID {wheelId}.", wheelId);
        return await _movieRepository.GetByWheelAsync(wheelId);
    }

    public async Task SetMovieEliminatedAsync(int wheelId, int movieId, bool eliminated)
    {
        Logger.LogInformation("Setting movie with ID {movieId} as {status} for wheel ID {wheelId}.", movieId, eliminated ? "eliminated" : "not eliminated", wheelId);
        var moviesOnWheel = await _movieRepository.GetByWheelAsync(wheelId);
        if (!moviesOnWheel.Any(m => m.Id == movieId))
            throw new InvalidOperationException("Movie is not part of this wheel.");

        await _movieRepository.SetEliminatedAsync(wheelId, movieId, eliminated);
    }

    public async Task SetMovieWatchedDateAsync(int wheelId, int movieId, DateOnly? watchedDate)
    {
        Logger.LogInformation("Setting watched date for movie with ID {movieId} on wheel ID {wheelId}.", movieId, wheelId);
        var moviesOnWheel = await _movieRepository.GetByWheelAsync(wheelId);
        if (!moviesOnWheel.Any(m => m.Id == movieId))
            throw new InvalidOperationException("Movie is not part of this wheel.");

        if (watchedDate.HasValue && watchedDate.Value > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentOutOfRangeException(nameof(watchedDate), "Watched date cannot be in the future.");

        await _movieRepository.SetWatchedDateAsync(wheelId, movieId, watchedDate);
    }

    public async Task<IEnumerable<Movie>> GetRemainingMoviesByWheelAsync(int wheelId)
    {
        Logger.LogInformation("Fetching remaining movies for wheel ID {wheelId}.", wheelId);
        return await _movieRepository.GetRemainingByWheelAsync(wheelId);
    }

    public async Task<IEnumerable<Movie>> GetLastWatchedMoviesAsync(int wheelId, int count)
    {
        Logger.LogInformation("Fetching last watched movies for wheel ID {wheelId}.", wheelId);
        return await _movieRepository.GetLastWatchedAsync(wheelId, count);
    }

    private async Task<bool> MovieExistsAsync(string title, string director, int year, int? tmdbId)
    {
        Logger.LogInformation("Checking if movie exists with title:{title}, director:{director}, year:{year}, tmdbId:{tmdbId}.", title, director, year, tmdbId);
        return await _movieRepository.MovieExistsAsync(title, director, year, tmdbId);
    }

    public async Task EnrichMoviesAsync(IEnumerable<Movie> movies)
    {
        Logger.LogInformation("Starting enrichment process for {movieCount} movies.", movies.Count());
        if (movies == null)
            return;

        foreach (var movie in movies)
        {
            Logger.LogInformation("Processing enrichment for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
            try
            {
                if (movie.TmdbId <= 0)
                    continue;
                    Logger.LogDebug("Movie ID {movieId} has valid TMDb ID {tmdbId}, checking for enrichment needs.", movie.Id, movie.TmdbId);

                bool needsMovieEnrichment = NeedsEnrichment(movie);
                bool needsProviderEnrichment = movie.WatchProviders?.Any() != true;

                if (!needsMovieEnrichment && !needsProviderEnrichment)
                    continue;

                if (needsMovieEnrichment)
                    await EnrichMovieDetailsAsync(movie);

                if (needsProviderEnrichment)
                    await EnrichMovieProvidersAsync(movie);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Enrichment failed for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
            }
        }
    }
    private async Task EnrichMovieDetailsAsync(Movie movie)
    {
        Logger.LogInformation("Enriching movie details for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
        var fetchedMovie = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId);
        if (fetchedMovie != null)
        {
            movie.Genre = string.IsNullOrWhiteSpace(movie.Genre) || movie.Genre == "Unknown Genre"
                ? fetchedMovie.Genre
                : movie.Genre;

            movie.Director = string.IsNullOrWhiteSpace(movie.Director) || movie.Director == "Unknown Director"
                ? fetchedMovie.Director
                : movie.Director;

            movie.Cast = string.IsNullOrWhiteSpace(movie.Cast) || movie.Cast == "Unknown Cast"
                ? fetchedMovie.Cast
                : movie.Cast;

            movie.DurationInMinutes = movie.DurationInMinutes <= 0
                ? fetchedMovie.DurationInMinutes
                : movie.DurationInMinutes;

            movie.Synopsis = string.IsNullOrWhiteSpace(movie.Synopsis) || movie.Synopsis == "Unknown Synopsis"
                ? fetchedMovie.Synopsis
                : movie.Synopsis;

            movie.PosterUrl = string.IsNullOrWhiteSpace(movie.PosterUrl)
                ? fetchedMovie.PosterUrl
                : movie.PosterUrl;

            movie.TmdbRating = movie.TmdbRating <= 0
                ? fetchedMovie.TmdbRating
                : movie.TmdbRating;
            await UpdateMovieAsync(movie);
            Logger.LogInformation("Enriched movie details for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
        }
    }

    private async Task EnrichMovieProvidersAsync(Movie movie)
    {
        Logger.LogInformation("Enriching watch providers for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
        var providersResponse = await _tmdbService.GetMovieWatchProvidersAsync(movie.TmdbId);
        var providerResults = providersResponse?.Results;

        if (providerResults != null && providerResults.Count > 0)
        {
            const string PreferredWatchRegion = "NL";
            var nlEntry = providerResults
                .FirstOrDefault(entry => string.Equals(entry.Key, PreferredWatchRegion, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(nlEntry.Key))
            {
                var nlProviders = nlEntry.Value;
                await _watchProviderService.ClearProvidersForMovieAsync(movie.Id);

                if (nlProviders.FlatRate != null)
                    await _watchProviderService.UpdateMovieProvidersAsync(movie.Id, nlProviders.FlatRate, "flatrate");
                if (nlProviders.Rent != null)
                    await _watchProviderService.UpdateMovieProvidersAsync(movie.Id, nlProviders.Rent, "rent");
                if (nlProviders.Buy != null)
                    await _watchProviderService.UpdateMovieProvidersAsync(movie.Id, nlProviders.Buy, "buy");
                Logger.LogInformation("Enriched watch providers for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
            }
        }
        else
        {
            Logger.LogInformation("No watch provider data found for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
        }
    }

    private static bool NeedsEnrichment(Movie movie)
    {
        return string.IsNullOrWhiteSpace(movie.Genre)
               || movie.Genre == "Unknown Genre"
               || string.IsNullOrWhiteSpace(movie.Director)
               || movie.Director == "Unknown Director"
               || string.IsNullOrWhiteSpace(movie.Cast)
               || movie.Cast == "Unknown Cast"
               || movie.DurationInMinutes <= 0
               || string.IsNullOrWhiteSpace(movie.Synopsis)
               || movie.Synopsis == "Unknown Synopsis"
               || string.IsNullOrWhiteSpace(movie.PosterUrl)
               || movie.TmdbRating <= 0;
    }
}