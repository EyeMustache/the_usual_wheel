using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces; 

namespace TheUsualWheelProject.Repositories;

public class WheelRepository : GenericRepository<Wheel, int>, IWheelRepository
{
    public WheelRepository(string conString) : base("Wheel", conString) { }

    public async Task<IEnumerable<WheelMovie>> GetMoviesByWheelIdAsync(int wheelId)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"SELECT *
                      FROM WheelMovie
                      WHERE WheelId = @WheelId";
        var param = new { WheelId = wheelId };
        return await connection.QueryAsync<WheelMovie>(query, param);
    }

    public async Task AddMovieToWheelAsync(int wheelId, int movieId)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"INSERT INTO WheelMovie (WheelId, MovieId, Eliminated)
                      VALUES (@WheelId, @MovieId, 0)";
        var param = new { WheelId = wheelId, MovieId = movieId };
        await connection.ExecuteAsync(query, param);
    }
}