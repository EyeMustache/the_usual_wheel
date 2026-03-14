using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Repositories;

public class WatchProviderRepository : GenericRepository<WatchProvider, int>, IWatchProviderRepository
{
    public WatchProviderRepository(string conString) : base("WatchProvider", conString) { }

    public async Task<WatchProvider?> GetByProviderIdAsync(int providerId)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        var query = @"SELECT * FROM WatchProvider WHERE ProviderId = @ProviderId LIMIT 1";
        var param = new { ProviderId = providerId };
        return await connection.QueryFirstOrDefaultAsync<WatchProvider>(query, param);
    }

    public async Task UpsertAsync(WatchProvider provider)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO WatchProvider (ProviderId, Name, LogoUrl, DisplayPriority)
            VALUES (@ProviderId, @Name, @LogoUrl, @DisplayPriority)
            ON CONFLICT(ProviderId) DO UPDATE SET
                Name = excluded.Name,
                LogoUrl = excluded.LogoUrl,
                DisplayPriority = excluded.DisplayPriority;";

        await connection.ExecuteAsync(query, provider);
    }
}
