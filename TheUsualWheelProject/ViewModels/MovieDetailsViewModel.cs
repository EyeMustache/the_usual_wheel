using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;
using CommunityToolkit.Mvvm.ComponentModel;


namespace TheUsualWheelProject.ViewModels;

public class MovieDetailsViewModel : ObservableObject
{
    private readonly MovieService _movieService;
    private readonly TmdbService _tmdbService;

    public Models.Movie? DbMovie { get; private set; }
    public TmdbMovie? TmdbMovie { get; private set; }
    public bool IsInDatabase => DbMovie != null;

    public MovieDetailsViewModel(MovieService movieService, TmdbService tmdbService)
    {
        _movieService = movieService;
        _tmdbService = tmdbService;
    }

    public async Task LoadAsync(int tmdbId)
    {
        DbMovie = await _movieService.GetMovieByTmdbIdAsync(tmdbId);
        if (DbMovie == null)
        {
            TmdbMovie = await _tmdbService.GetTmdbMovieDetailsAsync(tmdbId);
        }
        else
        {
            TmdbMovie = null;
        }
        OnPropertyChanged(nameof(DbMovie));
        OnPropertyChanged(nameof(TmdbMovie));
        OnPropertyChanged(nameof(IsInDatabase));
    }
}