using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.ViewModels;

public class WheelViewModel : LoggingBase<WheelViewModel>
{
    private const string PreferredWatchRegion = "NL";

    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;
    private readonly WatchProviderService _watchProviderService;

    public event Action? DataUpdated;

    public Wheel? CurrentWheel { get; private set; }
    public List<Movie>? Movies { get; private set; } = [];
    public List<WheelMovie>? WheelMovies { get; private set; }
    public bool IsHidden { get; set; } = false;
    public bool HasPeaked => IsHidden && WheelMovies?.Count(m => !m.IsEliminated) == 1;

    private bool _isEnriching;
    public bool IsEnriching
    {
        get => _isEnriching;
        private set
        {
            if (_isEnriching != value)
            {
                _isEnriching = value;
                Logger.LogInformation($"IsEnriching changed to {value}");
            }
        }
    }

    public WheelViewModel(
        WheelService wheelService,
        MovieService movieService,
        WatchProviderService watchProviderService,
        ILogger<WheelViewModel> logger) : base(logger)
    {
        _wheelService = wheelService;
        _movieService = movieService;
        _watchProviderService = watchProviderService;
    }

    public async Task LoadWheelAsync(int id)
    {
        // Fetch the wheel
        CurrentWheel = await _wheelService.GetWheelByIdAsync(id);

        // Load WheelMovies
        WheelMovies = (await _wheelService.GetWheelMoviesAsync(id)).ToList();

        // Load Movie details for each WheelMovie
        Movies = new List<Movie>();
        foreach (var wm in WheelMovies)
        {
            Logger.LogInformation("Loading movie details for Movie ID {movieId} in Wheel ID {wheelId}.", wm.MovieId, id);
            var movie = await _movieService.GetMovieByIdAsync(wm.MovieId);
            if (movie != null)
            {
                movie.WatchProviders = (await _watchProviderService.GetProvidersByMovieAsync(movie.Id)).ToList();
                Movies.Add(movie);
            }
        }

        DataUpdated?.Invoke();

        // await EnrichMoviesAsync();
    }

    private async Task EnrichMoviesAsync()
    {
        if (WheelMovies == null || !WheelMovies.Any())
            return;

        IsEnriching = true;
        DataUpdated?.Invoke();

        try
        {
            await _movieService.EnrichMoviesAsync(Movies);
            foreach (var movie in Movies)
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