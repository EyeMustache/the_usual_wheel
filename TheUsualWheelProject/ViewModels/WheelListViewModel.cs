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
        public int ActiveMovies => TotalMovies - EliminatedMovies;
    }

    private readonly WheelService _wheelService;
    public ObservableCollection<WheelCardInfo> Wheels { get; private set; } = new();

    public WheelListViewModel(WheelService wheel)
    {
        _wheelService = wheel;
    }

    public async Task LoadWheelAsync()
    {
        Wheels.Clear();
        var wheels = await _wheelService.GetAllWheelsAsync();
        foreach (Wheel wheel in wheels)
        {
            System.Diagnostics.Debug.WriteLine($"Loaded wheel: {wheel.Name}");

            var wheelMovies = (await _wheelService.GetWheelMoviesAsync(wheel.Id)).ToList();
            var eliminatedCount = wheelMovies.Count(wm => wm.IsEliminated);

            Wheels.Add(new WheelCardInfo
            {
                Wheel = wheel,
                TotalMovies = wheelMovies.Count,
                EliminatedMovies = eliminatedCount
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
}