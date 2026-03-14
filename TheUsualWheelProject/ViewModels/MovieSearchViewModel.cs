using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject.ViewModels;

public class MovieSearchViewModel
{
    public readonly MovieService _movieService;
    public readonly TmdbService _tmdbService;
    public ObservableCollection<Movie> SearchResults { get; set; } = [];
    public string? SearchQuery { get; set; } = string.Empty;
    public bool IsSearching { get; set; }
    public bool HasSearched { get; set; }
    public MovieSearchViewModel(MovieService movieService, TmdbService tmdbService)
    {
        _movieService = movieService;
        _tmdbService = tmdbService;
    }


    public async Task SearchMoviesAsync()
    {
        IsSearching = true;
        HasSearched = false;
        SearchResults.Clear();

        var tmdbResults = await _tmdbService.SearchMoviesAsync(SearchQuery);
        if (tmdbResults != null)
        {
            foreach (var tmdbMovie in tmdbResults)
            {
                var details = await _tmdbService.GetMovieDetailsAsync(tmdbMovie.Id);
                if (details != null)
                    SearchResults.Add(details);
            }
        }

        IsSearching = false;
        HasSearched = true;
    }
}