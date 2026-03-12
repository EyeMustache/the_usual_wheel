using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using System.Linq;

namespace TheUsualWheelProject.Services;

public class MovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        return await _movieRepository.GetAll();
    }

    public async Task<Movie?> GetMovieByIdAsync(int id)
    {
        return await _movieRepository.GetById(id);
    }

    public async Task AddMovieAsync(Movie movie)
    {
        if (await MovieExistsAsync(movie.Title, movie.Director, movie.Year, movie.TmdbId))
            throw new InvalidOperationException("Movie already exists.");

        await _movieRepository.Insert(movie);
    }

    public async Task UpdateMovieAsync(Movie movie)
    {
        var existingMovie = await _movieRepository.GetById(movie.Id);
        if (existingMovie == null)
            throw new InvalidOperationException("Movie does not exist.");

        if (movie.Id != existingMovie.Id)
            throw new InvalidOperationException("Movie ID mismatch.");

        await _movieRepository.Update(movie);

    }

    public async Task DeleteMovieAsync(int id)
    {
        var movie = await _movieRepository.GetById(id);
        if (movie == null)
            throw new InvalidOperationException("Movie does not exist.");

        await _movieRepository.Delete(id);
    }

    public async Task<IEnumerable<Movie>> GetMoviesByWheelAsync(int wheelId)
    {
        return await _movieRepository.GetByWheelAsync(wheelId);
    }

    public async Task SetMovieEliminatedAsync(int wheelId, int movieId, bool eliminated)
    {
        var moviesOnWheel = await _movieRepository.GetByWheelAsync(wheelId);
        if (!moviesOnWheel.Any(m => m.Id == movieId))
            throw new InvalidOperationException("Movie is not part of this wheel.");

        await _movieRepository.SetEliminatedAsync(wheelId, movieId, eliminated);
    }

    public async Task SetMovieWatchedDateAsync(int wheelId, int movieId, DateOnly? watchedDate)
    {
        var moviesOnWheel = await _movieRepository.GetByWheelAsync(wheelId);
        if (!moviesOnWheel.Any(m => m.Id == movieId))
            throw new InvalidOperationException("Movie is not part of this wheel.");

        if (watchedDate.HasValue && watchedDate.Value > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentOutOfRangeException(nameof(watchedDate), "Watched date cannot be in the future.");

        await _movieRepository.SetWatchedDateAsync(wheelId, movieId, watchedDate);
    }

    public async Task<IEnumerable<Movie>> GetRemainingMoviesByWheelAsync(int wheelId)
    {
        return await _movieRepository.GetRemainingByWheelAsync(wheelId);
    }

    public async Task<IEnumerable<Movie>> GetLastWatchedMoviesAsync(int wheelId, int count)
    {
        return await _movieRepository.GetLastWatchedAsync(wheelId, count);
    }

    private async Task<bool> MovieExistsAsync(string title, string director, int year, int? tmdbId)
    {
        return await _movieRepository.MovieExistsAsync(title, director, year, tmdbId);
    }
}