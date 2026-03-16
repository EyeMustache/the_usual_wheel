using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IWheelRepository : IGenericRepository<Wheel, int>
{
    Task<IEnumerable<WheelMovie>> GetMoviesByWheelIdAsync(int wheelId);
    Task AddMovieToWheelAsync(int wheelId, int movieId);
}