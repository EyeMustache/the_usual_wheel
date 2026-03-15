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
    public List<Movie>? WheelMovies { get; private set; } = [];
    public bool IsEnriching { get; private set; }

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
        
        // Offload the database loops to a background thread so the UI doesn't freeze
        WheelMovies = await Task.Run(async () => 
        {
            var movies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();
            
            foreach (var movie in movies)
            {
                movie.WatchProviders = (await _watchProviderService.GetProvidersByMovieAsync(movie.Id)).ToList();
            }
            
            return movies;
        });

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
            await _movieService.EnrichMoviesAsync(WheelMovies);
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