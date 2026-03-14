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

    // Calls enrichment logic in MovieService, updates UI state
    private async Task EnrichMissingMovieDataAsync()
    {
        if (WheelMovies == null || !WheelMovies.Any())
            return;

        IsEnriching = true;
        DataUpdated?.Invoke();

        try
        {
            await _movieService.EnrichMoviesAsync(WheelMovies);
            // Refresh providers after enrichment
            foreach (var movie in WheelMovies)
            {
                movie.WatchProviders = (await _watchProviderService.GetProvidersByMovieAsync(movie.Id)).ToList();
            }
            DataUpdated?.Invoke();
        }
        finally
        {
            IsEnriching = false;
            DataUpdated?.Invoke();
        }
    }

}