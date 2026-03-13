using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class WheelViewModel
{
    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;

    public Wheel? CurrentWheel { get; private set; }
    public List<Movie>? WheelMovies { get; private set; } = [];

    public WheelViewModel(WheelService wheelService, MovieService movieService)
    {
        _wheelService = wheelService;
        _movieService = movieService;
    }

    public async Task LoadWheelAsync(int id)
    {
        CurrentWheel = await _wheelService.GetWheelByIdAsync(id);
        WheelMovies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();
    }
}