using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IMovieProviderRepository : IGenericRepository<MovieProvider, int>
{
    Task<IEnumerable<WatchProvider>> GetProvidersByMovieIdAsync(int movieId);
    Task LinkProviderToMovieAsync(int movieId, int providerId, string type);
    Task ClearProvidersForMovieAsync(int movieId);
}
