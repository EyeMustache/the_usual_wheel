using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.ViewModels;

public class WheelViewModel : LoggingBase<WheelViewModel>
{
    private const string PreferredWatchRegion = "NL";

    private readonly TmdbService _tmdbService;
    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;
    private readonly WatchProviderService _watchProviderService;

    public event Action? DataUpdated;

    public Wheel? CurrentWheel { get; private set; }
    public List<Movie>? WheelMovies { get; private set; } = [];
    public bool IsEnriching { get; private set; }

    public WheelViewModel(
        WheelService wheelService,
        MovieService movieService,
        TmdbService tmdbService,
        WatchProviderService watchProviderService,
        ILogger<WheelViewModel> logger) : base(logger)
    {
        _wheelService = wheelService;
        _movieService = movieService;
        _tmdbService = tmdbService;
        _watchProviderService = watchProviderService;
    }

    public async Task LoadWheelAsync(int id)
    {
        CurrentWheel = await _wheelService.GetWheelByIdAsync(id);
        WheelMovies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();

        foreach (var movie in WheelMovies)
        {
            movie.WatchProviders = (await _watchProviderService.GetProvidersByMovieAsync(movie.Id)).ToList();
        }

        DataUpdated?.Invoke();

        _ = EnrichMissingMovieDataAsync();
    }

    // This method should be moved to the TmdbService, but for now it's here.
    private async Task EnrichMissingMovieDataAsync()
    {
        if (WheelMovies == null || !WheelMovies.Any())
            return;

        IsEnriching = true;
        DataUpdated?.Invoke();

        try
        {
            foreach (var movie in WheelMovies)
            {
                try
                {
                    if (movie.TmdbId <= 0)
                        continue;

                    var needsMovieEnrichment = NeedsEnrichment(movie);
                    var needsProviderEnrichment = movie.WatchProviders?.Any() != true;

                    if (!needsMovieEnrichment && !needsProviderEnrichment)
                        continue;

                    if (needsMovieEnrichment)
                    {
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

                            await _movieService.UpdateMovieAsync(movie);
                        }
                    }

                    if (needsProviderEnrichment)
                    {
                        var providersResponse = await _tmdbService.GetMovieWatchProvidersAsync(movie.TmdbId);
                        var providerResults = providersResponse?.Results;

                        if (providerResults == null || providerResults.Count == 0)
                        {
                            Logger.LogInformation("No watch provider results returned for TMDb ID {tmdbId}.", movie.TmdbId);
                        }
                        else
                        {
                            var nlEntry = providerResults
                                .FirstOrDefault(entry => string.Equals(entry.Key, PreferredWatchRegion, StringComparison.OrdinalIgnoreCase));

                            if (string.IsNullOrWhiteSpace(nlEntry.Key))
                            {
                                Logger.LogInformation(
                                    "No {region} watch provider data for TMDb ID {tmdbId}. Available regions: {regions}.",
                                    PreferredWatchRegion,
                                    movie.TmdbId,
                                    string.Join(", ", providerResults.Keys.OrderBy(region => region)));
                            }
                            else
                            {
                                var nlProviders = nlEntry.Value;
                                var flatrateCount = nlProviders.FlatRate?.Count ?? 0;
                                var rentCount = nlProviders.Rent?.Count ?? 0;
                                var buyCount = nlProviders.Buy?.Count ?? 0;

                                Logger.LogInformation(
                                    "Fetched {region} providers for TMDb ID {tmdbId}: flatrate={flatrateCount}, rent={rentCount}, buy={buyCount}.",
                                    PreferredWatchRegion,
                                    movie.TmdbId,
                                    flatrateCount,
                                    rentCount,
                                    buyCount);

                                await _watchProviderService.ClearProvidersForMovieAsync(movie.Id);

                                if (nlProviders.FlatRate != null)
                                    await _watchProviderService.UpdateMovieProvidersAsync(movie.Id, nlProviders.FlatRate, "flatrate");
                                if (nlProviders.Rent != null)
                                    await _watchProviderService.UpdateMovieProvidersAsync(movie.Id, nlProviders.Rent, "rent");
                                if (nlProviders.Buy != null)
                                    await _watchProviderService.UpdateMovieProvidersAsync(movie.Id, nlProviders.Buy, "buy");
                            }
                        }

                        movie.WatchProviders = (await _watchProviderService.GetProvidersByMovieAsync(movie.Id)).ToList();
                        Logger.LogInformation(
                            "Loaded {providerCount} watch providers for movie ID {movieId} after enrichment.",
                            movie.WatchProviders?.Count() ?? 0,
                            movie.Id);
                    }

                    DataUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Provider enrichment failed for movie ID {movieId} with TMDb ID {tmdbId}.", movie.Id, movie.TmdbId);
                }
            }
        }
        finally
        {
            IsEnriching = false;
            DataUpdated?.Invoke();
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