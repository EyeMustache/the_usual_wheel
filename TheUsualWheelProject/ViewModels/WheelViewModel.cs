using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.ViewModels;

[INotifyPropertyChanged]
public partial class WheelViewModel : LoggingBase<WheelViewModel>
{
    private const string PreferredWatchRegion = "NL";
    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;
    private readonly WatchProviderService _watchProviderService;

    public event Action? DataUpdated;

    public Wheel? CurrentWheel { get; private set; }
    public List<Movie>? Movies { get; private set; } = [];
    public List<WheelMovie>? WheelMovies { get; private set; }
    private Movie? _lastRemovedMovie;
    public Movie? LastRemovedMovie
    {
        get => _lastRemovedMovie;
        set
        {
            if (_lastRemovedMovie != value)
            {
                _lastRemovedMovie = value;
                OnPropertyChanged(nameof(LastRemovedMovie));
            }
        }
    }


    [ObservableProperty]
    private ObservableCollection<Movie> _activeSessionMovies = new();

    [ObservableProperty]
    private double _currentRotation;

    [ObservableProperty]
    private bool _isSpinning;
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

        Logger.LogInformation($"WheelMovies loaded: {WheelMovies.Count}, Movies loaded: {Movies.Count}");

        // Build the session movies list with logging for each mapping
        var sessionMovies = new ObservableCollection<Movie>();
        foreach (var wm in WheelMovies.Where(m => !m.IsEliminated))
        {
            var movie = Movies.FirstOrDefault(m => m.Id == wm.MovieId);
            if (movie != null)
            {
                sessionMovies.Add(movie);
                Logger.LogInformation($"Added movie to session: {movie.Title} (MovieId={movie.Id}) for WheelMovieId={wm.Id}");
            }
            else
            {
                Logger.LogWarning($"No match for WheelMovie.MovieId={wm.MovieId} in Movies list.");
            }
        }

        ActiveSessionMovies = sessionMovies;
        Logger.LogInformation($"ActiveSessionMovies count after load: {ActiveSessionMovies.Count}");
        CurrentRotation = 0;
        DataUpdated?.Invoke();
    }

    [RelayCommand]
    public async Task SpinAsync()
    {
        if (IsSpinning || ActiveSessionMovies.Count <= 1 || CurrentWheel == null) return;

        IsSpinning = true;

        var random = new Random();
        // Add 3-5 full rotations (1080 - 1800 degrees) plus a random offset
        double extraRotation = random.Next(1080, 1800) + random.Next(0, 360);
        CurrentRotation += extraRotation;

        // Wait for the 5-second CSS transition to finish
        await Task.Delay(5000);

        // Calculate winning/landed movie
        double normalizedRotation = CurrentRotation % 360;
        int count = ActiveSessionMovies.Count;
        
        // The pointer is at the top (0 degrees or 360/0 in CSS rotate space depending on implementation).
        // Since we'll draw segments starting from the top moving clockwise, rotating the wheel clockwise
        // means the segment that ends up under the top pointer comes from the counter-clockwise direction.
        // For simplicity, let's normalize angle to index:
        // Angle of segment i is i * (360/count).
        // Angle landed on the pointer is 360 - normalizedRotation (if starting at top and rotating clockwise).
        double pointerAngle = (360 - normalizedRotation) % 360;
        int landedIndex = (int)(pointerAngle / (360f / count)) % count;

        var eliminatedMovie = ActiveSessionMovies[landedIndex];
        LastRemovedMovie = eliminatedMovie;
        ActiveSessionMovies.Remove(eliminatedMovie);

        if (ActiveSessionMovies.Count == 1)
        {
            var winner = ActiveSessionMovies[0];
            Logger.LogInformation($"Wheel finished! Winner is {winner.Title}. Marking the rest as eliminated.");
            
            // Mark all eliminated movies in the database
            foreach (var movie in Movies)
            {
                if (movie.Id != winner.Id && WheelMovies.Any(wm => wm.MovieId == movie.Id && !wm.IsEliminated))
                {
                    await _wheelService.UpdateMovieEliminationStatusAsync(CurrentWheel.Id, movie.Id, true);
                }
            }
        }

        IsSpinning = false;
        DataUpdated?.Invoke();
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

    public async Task ResetSessionAsync()
    {
        if (CurrentWheel == null) return;
        Logger.LogInformation($"Resetting session for wheel ID {CurrentWheel.Id}. Marking all movies as not eliminated.");
        // Mark all movies as not eliminated in the database
        foreach (var wm in WheelMovies)
        {
            if (wm.IsEliminated)
            {
                Logger.LogInformation($"Resetting elimination status for movie {wm.MovieId} in wheel {CurrentWheel.Id}.");
                await _wheelService.UpdateMovieEliminationStatusAsync(CurrentWheel.Id, wm.MovieId, isEliminated: false);
            }
        }

        // Reload the wheel session
        await LoadWheelAsync(CurrentWheel.Id);
    }
}

// public class WheelSlice
// {
//     public int MovieId { get; set; }
//     public required string Title { get; set; }
//     public bool IsEliminated { get; set; }
// }
