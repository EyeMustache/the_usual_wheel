using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;

public class MainPageViewModel
{
    public ObservableCollection<Wheel> Wheels { get; set; } = new();
    public ObservableCollection<Movie> Movies { get; set; } = new();

    public MainPageViewModel(IWheelRepository wheelRepo, IMovieRepository movieRepo)
    {
        var wheels = wheelRepo.GetAll().GetAwaiter().GetResult();
        foreach (var w in wheels) Wheels.Add(w);

        var movies = movieRepo.GetAll().GetAwaiter().GetResult();
        foreach (var m in movies) Movies.Add(m);
    }
}