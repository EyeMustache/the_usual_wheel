using TheUsualWheelProject.Models;
using TMDbLib.Objects.Movies;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IMovieRepository : IGenericRepository<Models.Movie, int>
{
    Task<IEnumerable<Models.Movie>> GetByWheelAsync(int wheelId); 
    Task SetEliminatedAsync(int wheelId, int movieId, bool eliminated); // Joint table
    Task SetWatchedDateAsync(int wheelId, int movieId, DateOnly? watchedDate); // Joint table
    Task<IEnumerable<Models.Movie>> GetRemainingByWheelAsync(int wheelId); // For the wheel spin
    Task<IEnumerable<Models.Movie>> GetLastWatchedAsync(int wheelId, int count); // For the history section, to show the last X watched movies for a wheel. For future
    Task<bool> MovieExistsAsync(string title, string director, int year, int? tmdbId); // To prevent duplicates when adding new movies
    Task<Models.Movie?> GetByTmdbIdAsync(int tmdbId); // To check if a movie already exists based on TMDb ID
}