using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

public class MainPageViewModel
{
    public ObservableCollection<Wheel> Wheels { get; set; } = new();
    public ObservableCollection<Movie> Movies { get; set; } = new();

    public MainPageViewModel() { }

    public async Task LoadDataAsync(WheelService wheelService, MovieService movieService)
    {
        Wheels.Clear();
        Movies.Clear();

        var wheelsTask = wheelService.GetAllWheelsAsync();
        var moviesTask = movieService.GetAllMoviesAsync();

        var wheels = await wheelsTask;
        var movies = await moviesTask;

        foreach (var w in wheels) Wheels.Add(w);
        foreach (var m in movies) Movies.Add(m);
    }
}