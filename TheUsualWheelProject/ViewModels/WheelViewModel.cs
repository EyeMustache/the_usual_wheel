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
    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;
    private readonly WatchProviderService _watchProviderService;

    public event Action? DataUpdated;

    public Wheel? CurrentWheel { get; private set; }

    [ObservableProperty]
    private ObservableCollection<WheelMovieItem> _allWheelMovies = new();

    public IEnumerable<WheelMovieItem> WatchedMovies => AllWheelMovies.Where(wm => wm.IsWatched);

    private WheelMovieItem? _lastRemovedMovie;
    public WheelMovieItem? LastRemovedMovie
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
    private ObservableCollection<WheelMovieItem> _activeSessionMovies = new();

    [ObservableProperty]
    private double _currentRotation;

    [ObservableProperty]
    private bool _isSpinning;
    
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
        try
        {
            var wheel = await _wheelService.GetWheelByIdAsync(id)
                ?? throw new InvalidOperationException($"Wheel with ID {id} not found.");
            
            var wheelMoviesDb = (await _wheelService.GetWheelMoviesAsync(id)).ToList();
            var moviesDb = (await _movieService.GetMoviesByWheelAsync(id)).ToList();
            
            CurrentWheel = wheel;

            Logger.LogInformation("WheelMovies loaded: {Count}, Movies loaded: {Count2}", wheelMoviesDb.Count, moviesDb.Count);

            var allItems = new ObservableCollection<WheelMovieItem>();
            var sessionMovies = new ObservableCollection<WheelMovieItem>();

            foreach (var wm in wheelMoviesDb)
            {
                var movie = moviesDb.FirstOrDefault(m => m.Id == wm.MovieId);
                if (movie != null)
                {
                    var item = new WheelMovieItem
                    {
                        Movie = movie,
                        WheelMovie = wm
                    };
                    
                    allItems.Add(item);

                    if (item.IsActive)
                    {
                        sessionMovies.Add(item);
                        Logger.LogInformation("Added movie to session: {Title} (MovieId={MovieId})", movie.Title, movie.Id);
                    }
                }
                else
                {
                    Logger.LogWarning("No match for WheelMovie.MovieId={MovieId} in Movies list.", wm.MovieId);
                }
            }

            AllWheelMovies = allItems;
            ActiveSessionMovies = sessionMovies;
            CurrentRotation = 0;
            DataUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadWheelAsync failed for wheel {WheelId}", id);
            throw;
        }
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
        var dbMovie = await _movieService.GetMovieByIdAsync(movieId);
        if (dbMovie == null)
        {
            Logger.LogError($"Movie with DB ID {movieId} not found.");
            throw new InvalidOperationException("Movie not found in database.");
        }
        await _wheelService.AddMovieToWheelAsync(dbMovie.Id, wheelId);
    }

    public async Task ToggleMovieWatchedStatusAsync(int wheelMovieId, bool isWatched)
    {
        if (CurrentWheel == null || AllWheelMovies == null) return;
        
        var item = AllWheelMovies.FirstOrDefault(i => i.WheelMovie.Id == wheelMovieId);
        if (item == null) return;

        await _wheelService.SetMovieWatchedStatusAsync(wheelMovieId, isWatched);

        // Update local object
        item.WheelMovie.WatchedDate = isWatched ? DateOnly.FromDateTime(DateTime.Now) : null;

        // Ensure UI updates properly - notify collections
        if (isWatched)
        {
            if (ActiveSessionMovies.Contains(item))
                ActiveSessionMovies.Remove(item);
        }
        else
        {
            if (!item.IsEliminated && !ActiveSessionMovies.Contains(item))
                ActiveSessionMovies.Add(item);
        }

        OnPropertyChanged(nameof(AllWheelMovies));
        OnPropertyChanged(nameof(WatchedMovies));
        OnPropertyChanged(nameof(ActiveSessionMovies));
        
        DataUpdated?.Invoke();
    }

    public async Task RemoveWheelMovieAsync(int wheelMovieId)
    {
        if (CurrentWheel == null || AllWheelMovies == null) return;

        var item = AllWheelMovies.FirstOrDefault(i => i.WheelMovie.Id == wheelMovieId);
        if (item == null) return;

        await _wheelService.RemoveMovieFromWheelAsync(item.WheelMovie.MovieId, item.WheelMovie.WheelId);

        AllWheelMovies.Remove(item);
        ActiveSessionMovies.Remove(item);

        OnPropertyChanged(nameof(AllWheelMovies));
        OnPropertyChanged(nameof(WatchedMovies));
        DataUpdated?.Invoke();
    }

    public async Task RemoveWheelMoviesAsync(IEnumerable<int> wheelMovieIds)
    {
        if (CurrentWheel == null || AllWheelMovies == null) return;

        var idsToRemove = wheelMovieIds.ToHashSet();
        await _wheelService.RemoveMoviesFromWheelAsync(idsToRemove);

        var itemsToRemove = AllWheelMovies.Where(i => idsToRemove.Contains(i.WheelMovie.Id)).ToList();
        
        foreach (var item in itemsToRemove)
        {
            AllWheelMovies.Remove(item);
            ActiveSessionMovies.Remove(item);
        }

        OnPropertyChanged(nameof(AllWheelMovies));
        OnPropertyChanged(nameof(WatchedMovies));
        DataUpdated?.Invoke();
    }

    public async Task UpdateWheelMovieOrderAsync(List<int> orderedWheelMovieIds)
    {
        if (AllWheelMovies == null) return;

        var ordered = orderedWheelMovieIds
            .Select(id => AllWheelMovies.FirstOrDefault(wm => wm.WheelMovie.Id == id))
            .Where(wm => wm != null)
            .Cast<WheelMovieItem>()
            .ToList();

        AllWheelMovies = new ObservableCollection<WheelMovieItem>(ordered);

        OnPropertyChanged(nameof(AllWheelMovies));
        DataUpdated?.Invoke();
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
        if (CurrentWheel == null || AllWheelMovies == null) return;

        foreach (var item in AllWheelMovies)
        {
            if (item.IsEliminated)
            {
                Logger.LogInformation($"Resetting elimination status for movie {item.Movie.Id} in wheel {CurrentWheel.Id}.");
                await _wheelService.UpdateMovieEliminationStatusAsync(CurrentWheel.Id, item.Movie.Id, isEliminated: false);
            }
        }

        await LoadWheelAsync(CurrentWheel.Id);
    }

    public (double finalRotation, int selectedIndex) GetSpinResult(double currentRotation)
    {
        int count = ActiveSessionMovies.Count;
        var random = new Random();

        double normalizedCurrent = ((currentRotation % 360) + 360) % 360;

        int selectedIndex = random.Next(0, count);
        double sweepAngle = 360.0 / count;
        double minAngle = selectedIndex * sweepAngle;
        double maxAngle = (selectedIndex + 1) * sweepAngle;
        double targetAngle = minAngle + random.NextDouble() * (maxAngle - minAngle);
        double targetCurrentPosition = (targetAngle + normalizedCurrent) % 360;
        double offsetToTop = (360 - targetCurrentPosition) % 360;
        double fullSpins = 5 + random.Next(0, 3); 
        double finalRotation = fullSpins * 360 + offsetToTop;

        Logger.LogInformation($"SpinResult: selectedIndex={selectedIndex}, movie='{GetMovieToRemove(selectedIndex)?.Movie.Title}', finalRotation={finalRotation}, normalizedCurrent={normalizedCurrent}, targetAngle={targetAngle}");
        return (finalRotation, selectedIndex);
    }

    public void RemoveMovieAt(int selectedIndex, double finalRotation)
    {
        CurrentRotation += finalRotation;

        var eliminatedMovie = ActiveSessionMovies[selectedIndex];
        LastRemovedMovie = eliminatedMovie;
        ActiveSessionMovies.Remove(eliminatedMovie);
        IsSpinning = false;
        Logger.LogInformation($"RemoveMovieAt: selectedIndex={selectedIndex}, movie='{eliminatedMovie.Movie.Title}', finalRotation={finalRotation}, currentRotation={CurrentRotation}");
        DataUpdated?.Invoke();
    }

    public WheelMovieItem? GetMovieToRemove(int index)
    {
        if (index >= 0 && index < ActiveSessionMovies.Count)
            return ActiveSessionMovies[index];
        return null;
    }
}
