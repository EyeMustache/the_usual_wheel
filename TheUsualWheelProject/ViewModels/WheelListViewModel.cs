using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class WheelListViewModel
{
    public class WheelCardInfo
    {
        public Wheel Wheel { get; set; } = default!;
        public int TotalMovies { get; set; }
        public int EliminatedMovies { get; set; }
        public int WatchedMovies { get; set; }
        public int ActiveMovies => TotalMovies - WatchedMovies;
    }

    public class LastWatchedInfo
    {
        public string WheelName { get; set; } = "No movies watched yet";
        public string MovieTitle { get; set; } = "No movies watched yet";
        public DateOnly? WatchedDate { get; set; }
    }

    public LastWatchedInfo LastWatched { get; private set; } = new();

    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;
    public ObservableCollection<WheelCardInfo> Wheels { get; private set; } = new();
    public WheelMovie? LastWatchedWheelMovie {get; private set; }
    public IEnumerable<Wheel> AvailableWheels => Wheels.Select(x => x.Wheel);
    public bool IsWelcomeBackDismissed { get; private set; }

    public WheelListViewModel(WheelService wheel, MovieService movie)
    {
        _wheelService = wheel;
        _movieService = movie;
    }

    public async Task LoadWheelAsync()
    {
        Wheels.Clear();
        var wheels = await _wheelService.GetAllWheelsAsync();
        foreach (Wheel wheel in wheels)
        {
            var wheelMovies = (await _wheelService.GetWheelMoviesAsync(wheel.Id)).ToList();
            var eliminatedCount = wheelMovies.Count(wm => wm.IsEliminated);
            var watchedCount = wheelMovies.Count(wm => wm.WatchedDate != null);

            Wheels.Add(new WheelCardInfo
            {
                Wheel = wheel,
                TotalMovies = wheelMovies.Count,
                EliminatedMovies = eliminatedCount,
                WatchedMovies = watchedCount
            });
        }
    }

    public async Task AddWheelAsync(string name, string description)
    {
        var newWheel = new Wheel
        {
            Name = name,
            Description = description,
        };

        Wheel? createdWheel = await _wheelService.AddWheelAsync(newWheel);
        if (createdWheel != null)
        {
            Wheels.Add(new WheelCardInfo
            {
                Wheel = createdWheel,
                TotalMovies = 0,
                EliminatedMovies = 0
            });
        }
    }

    public async Task UpdateWheelAsync(int wheelId, string name, string description)
    {
        var target = Wheels.FirstOrDefault(w => w.Wheel.Id == wheelId);
        if (target == null) return;

        var updatedWheel = new Wheel
        {
            Id = wheelId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };

        await _wheelService.UpdateWheelAsync(updatedWheel);

        target.Wheel.Name = updatedWheel.Name;
        target.Wheel.Description = updatedWheel.Description;
    }

    public async Task DeleteWheelAsync(int wheelId)
    {
        await _wheelService.DeleteWheelAsync(wheelId);

        WheelCardInfo? target = Wheels.FirstOrDefault(w => w.Wheel.Id == wheelId);
        if (target != null)
        {
            Wheels.Remove(target);
        }
    }

    public async Task LoadLastWatchedWheelMovieAsync()
    {
        var lastWatchedWheelMovie = await _wheelService.GetLastWatchedWheelMovieAsync();
        if (lastWatchedWheelMovie == null)
        {
            LastWatched = new LastWatchedInfo();
            return;
        }

        var wheel = await _wheelService.GetWheelByIdAsync(lastWatchedWheelMovie.WheelId);
        var movie = await _movieService.GetMovieByIdAsync(lastWatchedWheelMovie.MovieId);

        LastWatched = new LastWatchedInfo
        {
            WheelName = wheel?.Name ?? "Unknown wheel",
            MovieTitle = movie?.Title ?? "Unknown movie",
            WatchedDate = lastWatchedWheelMovie.WatchedDate
        };
    }

    public void DismissWelcomeBack() => IsWelcomeBackDismissed = true;



}