using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class MovieViewModel
{
    private readonly MovieService _movieService;
    private readonly TmdbService _tmdbService;

    public object? CurrentMovie { get; private set; } // Can be either Models.Movie or TmdbMovie

    public MovieViewModel(MovieService movieService, TmdbService tmdbService)
    {
        _movieService = movieService;
        _tmdbService = tmdbService;
    }

    public async Task LoadMovieAsync(int id)
    {
        CurrentMovie = await _movieService.GetMovieByIdAsync(id);
    }

    public async Task LoadMovieByTmdbIdAsync(int tmdbId)
    {
        CurrentMovie = await _movieService.GetMovieOrTmdbMovieByTmdbIdAsync(tmdbId);
    }
}