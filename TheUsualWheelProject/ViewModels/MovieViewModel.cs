using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class MovieViewModel
{
    private readonly MovieService _movieService;
    private readonly TmdbService _tmdbService;

    public Movie? CurrentMovie { get; private set; }

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
        var localMovie = await _movieService.GetMovieByTmdbIdAsync(tmdbId);
        if (localMovie != null && localMovie.Id != 0)
            CurrentMovie = localMovie;
        else
            CurrentMovie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
    }
}