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
        Wheel wheel = await _wheelService.GetWheelByIdAsync(id) ?? throw new InvalidOperationException($"Wheel with ID {id} not found.");
        WheelMovies = (await _wheelService.GetWheelMoviesAsync(id)).ToList();
        Movies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();
        CurrentWheel = wheel;
    }

    private async Task AddMovieToWheelAsync(TmdbMovie tmdbMovie, int wheelId)
    {
        Logger.LogInformation($"Inserting movie with TMDb ID {tmdbMovie.TmdbId} and adding to wheel ID {wheelId}.");
        var movieToInsert = tmdbMovie.MapToDbMovie();
        await _movieService.AddMovieAsync(movieToInsert);
        var dbMovie = await _movieService.GetMovieByTmdbIdAsync(tmdbMovie.TmdbId);

        if (dbMovie != null)
        {
            await _wheelService.AddMovieToWheelAsync(dbMovie.Id, wheelId);
        }
        else
        {
            Logger.LogError($"Failed to add movie with TMDb ID {tmdbMovie.TmdbId} to wheel ID {wheelId} after insert.");
            throw new InvalidOperationException("Failed to add movie to wheel.");
        }
    }

    public async Task AddMovieToWheelByTmdbIdAsync(int tmdbId, int wheelId)
    {
        Logger.LogInformation($"Adding movie with TMDb ID {tmdbId} to wheel ID {wheelId} by TMDb ID.");
        var movieOrTmdb = await _movieService.GetMovieOrTmdbMovieByTmdbIdAsync(tmdbId);

        if (movieOrTmdb is Movie dbMovie)
        {
            Logger.LogInformation($"Movie with TMDb ID {tmdbId} found in DB as '{dbMovie.Title}'. Adding it to wheel ID {wheelId}.");
            await _wheelService.AddMovieToWheelAsync(dbMovie.Id, wheelId);
        }
        else if (movieOrTmdb is TmdbMovie tmdbMovie)
        {
            Logger.LogInformation($"Movie with TMDb ID {tmdbId} not found in DB. Fetched from TMDb as '{tmdbMovie.Title}'. Adding it to wheel ID {wheelId}.");
            await AddMovieToWheelAsync(tmdbMovie, wheelId);
        }
        else
        {
            Logger.LogError($"Failed to add movie with TMDb ID {tmdbId} to wheel ID {wheelId} because it could not be found in DB or fetched from TMDb.");
            throw new InvalidOperationException("Could not find or fetch movie.");
        }
    }

    public async Task AddMovieToWheelByMovieIdAsync(int movieId, int wheelId)
    {
        // Logger.LogInformation($"Adding movie with DB ID {movieId} to wheel ID {wheelId}.");
        var dbMovie = await _movieService.GetMovieByIdAsync(movieId);
        if (dbMovie == null)
        {
            Logger.LogError($"Movie with DB ID {movieId} not found.");
            throw new InvalidOperationException("Movie not found in database.");
        }
        await _wheelService.AddMovieToWheelAsync(dbMovie.Id, wheelId);
    }

    // public async Task EnrichMoviesBackgroundAsync()
    // {
    //     if (Movies == null || Movies.Count == 0) return;
    //     IsEnriching = true;
    //     await _movieService.EnrichMoviesAsync(Movies);
    //     IsEnriching = false;
    //     DataUpdated?.Invoke();
    // }

    // public async Task LoadWheelAsync(int id)
    // {
    //     // Fetch the wheel
    //     CurrentWheel = await _wheelService.GetWheelByIdAsync(id);
    //     Movies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();

    //     // Populate Slices
    //     Slices.Clear();
    //     foreach (var wm in WheelMovies)
    //     {
    //         var movieObj = Movies.FirstOrDefault(m => m.Id == wm.MovieId);
    //         if (movieObj != null)
    //         {
    //             Slices.Add(new WheelSlice
    //             {
    //                 MovieId = movieObj.Id,
    //                 Title = movieObj.Title,
    //                 IsEliminated = wm.IsEliminated
    //             });
    //         }
    //     }

    //     DataUpdated?.Invoke();
    // }
}

public class WheelSlice
{
    public int MovieId { get; set; }
    public required string Title { get; set; }
    public bool IsEliminated { get; set; }
}