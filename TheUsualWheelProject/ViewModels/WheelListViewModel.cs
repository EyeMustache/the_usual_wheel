using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class WheelListViewModel
{
    private readonly WheelService _wheelService;
    public ObservableCollection<Wheel> Wheels { get; private set; } = new();

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
            Wheels.Add(wheel);
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
            Wheels.Add(createdWheel);
        }
    }
}