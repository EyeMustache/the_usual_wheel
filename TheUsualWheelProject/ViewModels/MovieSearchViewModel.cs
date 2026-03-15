using System.Collections.ObjectModel;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Services;
using TMDbLib.Objects.Search;

namespace TheUsualWheelProject.ViewModels;

public class MovieSearchViewModel
{
    private readonly MovieService _movieService;
    private readonly TmdbService _tmdbService;

    // Configurable limit
    public int BatchSize { get; set; } = 5;

    // State Tracking
    private List<SearchMovie> _currentTmdbResults = new();
    private int _currentResultIndex = 0;
    private int _currentTmdbPage = 1;

    public ObservableCollection<Movie> SearchResults { get; set; } = [];
    public string? SearchQuery { get; set; } = string.Empty;
    public bool IsSearching { get; set; }
    public bool HasSearched { get; set; }
    public bool HasMoreResults { get; set; }

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
        _currentTmdbResults.Clear();
        _currentResultIndex = 0;
        _currentTmdbPage = 1;

        // Fetch the first page of 20 results
        var tmdbResults = await _tmdbService.SearchMoviesAsync(SearchQuery, _currentTmdbPage);
        
        if (tmdbResults != null && tmdbResults.Any())
        {
            _currentTmdbResults = tmdbResults;
            await ProcessNextBatchAsync();
        }

        IsSearching = false;
        HasSearched = true;
    }

    public async Task LoadMoreAsync()
    {
        IsSearching = true;

        // If we ran out of items in our list of 20, fetch the next page from TMDB
        if (_currentResultIndex >= _currentTmdbResults.Count)
        {
            _currentTmdbPage++;
            var nextResults = await _tmdbService.SearchMoviesAsync(SearchQuery, _currentTmdbPage);
            
            if (nextResults != null && nextResults.Any())
            {
                _currentTmdbResults.AddRange(nextResults);
            }
            else
            {
                HasMoreResults = false;
                IsSearching = false;
                return;
            }
        }

        await ProcessNextBatchAsync();
        IsSearching = false;
    }

    private async Task ProcessNextBatchAsync()
    {
        var batch = _currentTmdbResults.Skip(_currentResultIndex).Take(BatchSize).ToList();

        foreach (var tmdbMovie in batch)
        {
            var details = await _tmdbService.GetMovieDetailsAsync(tmdbMovie.Id);
            if (details != null)
            {
                SearchResults.Add(details);
            }
        }

        _currentResultIndex += batch.Count;
        
        HasMoreResults = batch.Count == BatchSize || _currentResultIndex < _currentTmdbResults.Count;
    }
}