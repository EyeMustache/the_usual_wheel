using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IWheelRepository : IGenericRepository<Wheel, int>
{
    Task<IEnumerable<WheelMovie>> GetMoviesByWheelIdAsync(int wheelId);
    Task AddMovieToWheelAsync(int wheelId, int movieId);
    Task UpdateMovieEliminationStatusAsync(int wheelId, int movieId, bool isEliminated);
    Task UpdateMovieWatchedDateAsync(int wheelMovieId, DateOnly? watchedDate);
    Task RemoveMovieFromWheelAsync(int movieId, int wheelId);
    Task RemovieMoviesFromWheelAsync(IEnumerable<int> wheelMovieIds);
    Task<WheelMovie?> GetLastWatchedWheelMovieAsync();
}