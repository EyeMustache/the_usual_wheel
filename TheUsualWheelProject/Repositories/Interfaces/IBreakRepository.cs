using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IBreakRepository : IGenericRepository<Break, int>
{
    Task<IEnumerable<Break>> GetBreaksByMovieIdAsync(int movieId);
    Task InsertBreaksAsync(IEnumerable<Break> breaks);
}