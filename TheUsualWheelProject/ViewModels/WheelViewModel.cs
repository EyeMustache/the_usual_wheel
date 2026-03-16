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

    // For logging to track enrichment status, aswell as to tell the program we loading data
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
        Movies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();
    }

    private async Task EnrichMoviesAsync()
    {
        if (Movies == null || !Movies.Any())
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

public class WheelSliceDto
{
    public int MovieId { get; set; }
    public required string Title { get; set; }
    public bool IsEliminated { get; set; }
}