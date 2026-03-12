using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;
using System.Collections.ObjectModel;

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
            Wheels.Add(wheel);
        }
    }   
}