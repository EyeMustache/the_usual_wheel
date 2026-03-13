using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class WheelViewModel
{
    private readonly TmdbService _tmdbService;
    private readonly WheelService _wheelService;
    private readonly MovieService _movieService;

    public event Action? DataUpdated;

    public Wheel? CurrentWheel { get; private set; }
    public List<Movie>? WheelMovies { get; private set; } = [];
    public bool IsEnriching { get; private set; }

    public WheelViewModel(WheelService wheelService, MovieService movieService, TmdbService tmdbService)
    {
        _wheelService = wheelService;
        _movieService = movieService;
        _tmdbService = tmdbService;
    }

    public async Task LoadWheelAsync(int id)
    {
        CurrentWheel = await _wheelService.GetWheelByIdAsync(id);
        WheelMovies = (await _movieService.GetMoviesByWheelAsync(id)).ToList();

        DataUpdated?.Invoke();

        _ = EnrichMissingMovieDataAsync();
    }

    private async Task EnrichMissingMovieDataAsync()
    {
        if (WheelMovies == null || !WheelMovies.Any())
            return;

        IsEnriching = true;
        DataUpdated?.Invoke();

        foreach (var movie in WheelMovies)
        {
            if (movie.TmdbId <= 0 || !NeedsEnrichment(movie))
                continue;

            var fetchedMovie = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId);
            if (fetchedMovie == null)
                continue;

            movie.Genre = string.IsNullOrWhiteSpace(movie.Genre) || movie.Genre == "Unknown Genre"
                ? fetchedMovie.Genre
                : movie.Genre;

            movie.Director = string.IsNullOrWhiteSpace(movie.Director) || movie.Director == "Unknown Director"
                ? fetchedMovie.Director
                : movie.Director;

            movie.Cast = string.IsNullOrWhiteSpace(movie.Cast) || movie.Cast == "Unknown Cast"
                ? fetchedMovie.Cast
                : movie.Cast;

            movie.DurationInMinutes = movie.DurationInMinutes <= 0
                ? fetchedMovie.DurationInMinutes
                : movie.DurationInMinutes;

            movie.Synopsis = string.IsNullOrWhiteSpace(movie.Synopsis) || movie.Synopsis == "Unknown Synopsis"
                ? fetchedMovie.Synopsis
                : movie.Synopsis;

            movie.PosterUrl = string.IsNullOrWhiteSpace(movie.PosterUrl)
                ? fetchedMovie.PosterUrl
                : movie.PosterUrl;

            movie.TmdbRating = movie.TmdbRating <= 0
                ? fetchedMovie.TmdbRating
                : movie.TmdbRating;

            await _movieService.UpdateMovieAsync(movie);
            DataUpdated?.Invoke();
        }

        IsEnriching = false;
        DataUpdated?.Invoke();
    }

    private static bool NeedsEnrichment(Movie movie)
    {
        return string.IsNullOrWhiteSpace(movie.Genre)
               || movie.Genre == "Unknown Genre"
               || string.IsNullOrWhiteSpace(movie.Director)
               || movie.Director == "Unknown Director"
               || string.IsNullOrWhiteSpace(movie.Cast)
               || movie.Cast == "Unknown Cast"
               || movie.DurationInMinutes <= 0
               || string.IsNullOrWhiteSpace(movie.Synopsis)
               || movie.Synopsis == "Unknown Synopsis"
               || string.IsNullOrWhiteSpace(movie.PosterUrl)
               || movie.TmdbRating <= 0;
    }
}