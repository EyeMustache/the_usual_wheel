using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public class MovieService : LoggingBase<MovieService>
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository, ILogger<MovieService> logger) : base(logger)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        Logger.LogInformation("Fetching all movies.");
        return await _movieRepository.GetAll();
    }

    public async Task<Movie?> GetMovieByIdAsync(int id)
    {
        Logger.LogInformation("Fetching movie by ID:{id}.", id);
        return await _movieRepository.GetById(id);
    }

    public async Task AddMovieAsync(Movie movie)
    {
        Logger.LogInformation("Adding new movie:{movie.Title} directed by {movie.Director} ({movie.Year}).", movie.Title, movie.Director, movie.Year);
        if (await MovieExistsAsync(movie.Title, movie.Director, movie.Year, movie.TmdbId))
            throw new InvalidOperationException("Movie already exists.");

        await _movieRepository.Insert(movie);
    }

    public async Task UpdateMovieAsync(Movie movie)
    {
        Logger.LogInformation("Updating movie with ID {movie.Id}.", movie.Id);
        var existingMovie = await _movieRepository.GetById(movie.Id);
        if (existingMovie == null)
            throw new InvalidOperationException("Movie does not exist.");

        if (movie.Id != existingMovie.Id)
            throw new InvalidOperationException("Movie ID mismatch.");

        await _movieRepository.Update(movie);

    }

    public async Task DeleteMovieAsync(int id)
    {
        Logger.LogInformation("Deleting movie with ID {id}.", id);
        var movie = await _movieRepository.GetById(id);
        if (movie == null)
            throw new InvalidOperationException("Movie does not exist.");

        await _movieRepository.Delete(id);
    }

    public async Task<IEnumerable<Movie>> GetMoviesByWheelAsync(int wheelId)
    {
        Logger.LogInformation("Fetching movies for wheel ID {wheelId}.", wheelId);
        return await _movieRepository.GetByWheelAsync(wheelId);
    }

    public async Task SetMovieEliminatedAsync(int wheelId, int movieId, bool eliminated)
    {
        Logger.LogInformation("Setting movie with ID {movieId} as {status} for wheel ID {wheelId}.", movieId, eliminated ? "eliminated" : "not eliminated", wheelId);
        var moviesOnWheel = await _movieRepository.GetByWheelAsync(wheelId);
        if (!moviesOnWheel.Any(m => m.Id == movieId))
            throw new InvalidOperationException("Movie is not part of this wheel.");

        await _movieRepository.SetEliminatedAsync(wheelId, movieId, eliminated);
    }

    public async Task SetMovieWatchedDateAsync(int wheelId, int movieId, DateOnly? watchedDate)
    {
        Logger.LogInformation("Setting watched date for movie with ID {movieId} on wheel ID {wheelId}.", movieId, wheelId);
        var moviesOnWheel = await _movieRepository.GetByWheelAsync(wheelId);
        if (!moviesOnWheel.Any(m => m.Id == movieId))
            throw new InvalidOperationException("Movie is not part of this wheel.");

        if (watchedDate.HasValue && watchedDate.Value > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentOutOfRangeException(nameof(watchedDate), "Watched date cannot be in the future.");

        await _movieRepository.SetWatchedDateAsync(wheelId, movieId, watchedDate);
    }

    public async Task<IEnumerable<Movie>> GetRemainingMoviesByWheelAsync(int wheelId)
    {
        Logger.LogInformation("Fetching remaining movies for wheel ID {wheelId}.", wheelId);
        return await _movieRepository.GetRemainingByWheelAsync(wheelId);
    }

    public async Task<IEnumerable<Movie>> GetLastWatchedMoviesAsync(int wheelId, int count)
    {
        Logger.LogInformation("Fetching last watched movies for wheel ID {wheelId}.", wheelId);
        return await _movieRepository.GetLastWatchedAsync(wheelId, count);
    }

    private async Task<bool> MovieExistsAsync(string title, string director, int year, int? tmdbId)
    {
        Logger.LogInformation("Checking if movie exists with title:{title}, director:{director}, year:{year}, tmdbId:{tmdbId}.", title, director, year, tmdbId);
        return await _movieRepository.MovieExistsAsync(title, director, year, tmdbId);
    }
}