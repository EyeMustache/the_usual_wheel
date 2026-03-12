using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TMDbLib.Objects.Movies;

namespace TheUsualWheelProject.Repositories;

public class MovieRepository : GenericRepository<Models.Movie, int>, IMovieRepository
{
    public MovieRepository(string conString) : base("Movie", conString) { }

    public async Task<IEnumerable<Models.Movie>> GetByWheelAsync(int wheelId)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"SELECT m.*
                      FROM {TableName} m
                      INNER JOIN WheelMovie wm ON m.Id = wm.MovieId
                      WHERE wm.WheelId = @WheelId";
        var param = new { WheelId = wheelId };
        return await connection.QueryAsync<Models.Movie>(query, param);
    }

    public async Task SetEliminatedAsync(int wheelId, int movieId, bool eliminated)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"UPDATE WheelMovie
                      SET Eliminated = @Eliminated
                      WHERE WheelId = @WheelId AND MovieId = @MovieId";
        var param = new { WheelId = wheelId, MovieId = movieId, Eliminated = eliminated };
        await connection.ExecuteAsync(query, param);
    }

    public async Task SetWatchedDateAsync(int wheelId, int movieId, DateOnly? watchedDate)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"UPDATE WheelMovie
                      SET WatchedDate = @WatchedDate
                      WHERE WheelId = @WheelId AND MovieId = @MovieId";
        var param = new { WheelId = wheelId, MovieId = movieId, WatchedDate = watchedDate };
        await connection.ExecuteAsync(query, param);
    }

    public async Task<IEnumerable<Models.Movie>> GetRemainingByWheelAsync(int wheelId)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"SELECT m.*
                      FROM {TableName} m
                      INNER JOIN WheelMovie wm ON m.Id = wm.MovieId
                      WHERE wm.WheelId = @WheelId AND wm.Eliminated = 0";
        var param = new { WheelId = wheelId };
        return await connection.QueryAsync<Models.Movie>(query, param);
    }

    public async Task<IEnumerable<Models.Movie>> GetLastWatchedAsync(int wheelId, int count)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"SELECT m.*
                      FROM {TableName} m
                      INNER JOIN WheelMovie wm ON m.Id = wm.MovieId
                      WHERE wm.WheelId = @WheelId AND wm.WatchedDate IS NOT NULL
                      ORDER BY wm.WatchedDate DESC
                      LIMIT @Count";
        var param = new { WheelId = wheelId, Count = count };
        return await connection.QueryAsync<Models.Movie>(query, param);
    }

    public async Task<bool> MovieExistsAsync(string title, string director, int year, int? tmdbId)
    {
        using var connection = new SqliteConnection(_conString);
        var query = @$"SELECT COUNT(1)
                      FROM {TableName}
                      WHERE Title = @Title AND Director = @Director AND Year = @Year AND (TmdbId = @TmdbId OR (@TmdbId IS NULL AND TmdbId IS NULL))";
        var param = new { Title = title, Director = director, Year = year, TmdbId = tmdbId };
        var count = await connection.ExecuteScalarAsync<int>(query, param);
        return count > 0;
    }
}
