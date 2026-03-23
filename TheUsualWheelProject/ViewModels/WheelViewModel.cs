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

    [RelayCommand]
    public async Task SpinAsync()
    {
        if (IsSpinning || ActiveSessionMovies.Count <= 1 || CurrentWheel == null) return;
        IsSpinning = true;

        var result = GetSpinResult(CurrentRotation);
        CurrentRotation += result.Item1;
        LastRemovedMovie = ActiveSessionMovies[result.Item2];

        IsSpinning = false;
        DataUpdated?.Invoke();
    }

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

    /// <summary>
    /// Calculates the spin so the pointer lands randomly within the selected slice, relative to the current wheel rotation.
    /// </summary>
    /// <param name="currentRotation">The current cumulative rotation of the wheel (in degrees).</param>
    /// <returns>(finalRotation, selectedIndex)</returns>
    public (double finalRotation, int selectedIndex) GetSpinResult(double currentRotation)
    {
        int count = ActiveSessionMovies.Count;
        var random = new Random();

        // Normalize current rotation to [0, 360)
        double normalizedCurrent = ((currentRotation % 360) + 360) % 360;

        int selectedIndex = random.Next(0, count);
        double sweepAngle = 360.0 / count;
        double minAngle = selectedIndex * sweepAngle;
        double maxAngle = (selectedIndex + 1) * sweepAngle;
        // Pick a random point within the slice
        double targetAngle = minAngle + random.NextDouble() * (maxAngle - minAngle);
        // Find where the target angle currently sits on the screen
        double targetCurrentPosition = (targetAngle + normalizedCurrent) % 360;
        // Calculate the exact degrees needed to push that position to the top
        double offsetToTop = (360 - targetCurrentPosition) % 360;
        // Add the full spins to make it look like a real spin
        double fullSpins = 5 + random.Next(0, 3); // 5-7 full spins
        double finalRotation = fullSpins * 360 + offsetToTop;

        Logger.LogInformation($"SpinResult: selectedIndex={selectedIndex}, movie='{GetMovieToRemove(selectedIndex)?.Title}', finalRotation={finalRotation}, normalizedCurrent={normalizedCurrent}, targetAngle={targetAngle}");
        return (finalRotation, selectedIndex);
    }

    public void RemoveMovieAt(int selectedIndex, double finalRotation)
    {
        // Update rotation (do not normalize, keep cumulative for smooth animation)
        CurrentRotation += finalRotation;

        var eliminatedMovie = ActiveSessionMovies[selectedIndex];
        LastRemovedMovie = eliminatedMovie;
        ActiveSessionMovies.Remove(eliminatedMovie);
        IsSpinning = false;
        Logger.LogInformation($"RemoveMovieAt: selectedIndex={selectedIndex}, movie='{eliminatedMovie.Title}', finalRotation={finalRotation}, currentRotation={CurrentRotation}");
        DataUpdated?.Invoke();
    }

    // Helper: Get the movie that would be removed for a given index (for debug display)
    public Movie? GetMovieToRemove(int index)
    {
        if (index >= 0 && index < ActiveSessionMovies.Count)
            return ActiveSessionMovies[index];
        return null;
    }

}