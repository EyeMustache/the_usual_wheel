using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IWatchProviderRepository : IGenericRepository<WatchProvider, int>
{
    Task<WatchProvider?> GetByProviderIdAsync(int providerId);
    Task UpsertAsync(WatchProvider provider);
}
