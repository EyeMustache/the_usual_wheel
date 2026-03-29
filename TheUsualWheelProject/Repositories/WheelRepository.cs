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
        var query = @$"INSERT INTO WheelMovie (WheelId, MovieId, IsEliminated)
                      VALUES (@WheelId, @MovieId, 0)";
        var param = new { WheelId = wheelId, MovieId = movieId };
        await connection.ExecuteAsync(query, param);
    }

    public async Task UpdateMovieEliminationStatusAsync(int wheelId, int movieId, bool isEliminated)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"UPDATE WheelMovie
                       SET IsEliminated = @IsEliminated
                       WHERE WheelId = @WheelId AND MovieId = @MovieId";
        var param = new { WheelId = wheelId, MovieId = movieId, IsEliminated = isEliminated ? 1 : 0 };
        await connection.ExecuteAsync(query, param);
    }

    public async Task RemoveMovieFromWheelAsync(int movieId, int wheelId)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"DELETE FROM WheelMovie
                       WHERE WheelId = @WheelId AND MovieId = @MovieId";
        var param = new { WheelId = wheelId, MovieId = movieId };
        await connection.ExecuteAsync(query, param);
    }

    public async Task RemovieMoviesFromWheelAsync(IEnumerable<int> wheelMovieIds)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"DELETE FROM WheelMovie
                       WHERE Id IN @WheelMovieIds";
        var param = new { WheelMovieIds = wheelMovieIds };
        await connection.ExecuteAsync(query, param);
    }
}