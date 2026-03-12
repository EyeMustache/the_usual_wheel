using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TMDbLib.Objects.Movies;

namespace TheUsualWheelProject.Repositories;

public class MovieRepository : GenericRepository<Models.Movie, int>, IMovieRepository
{
    public MovieRepository(string conString) : base("Movie", conString)
    { }

    public async Task<IEnumerable<Models.Movie>> GetByWheelAsync(int wheelId)
    {
        // using var connection = new SqliteConnection(_conString);
        // var query = @$"SELECT m.*
        //               FROM Movie m
        //               INNER JOIN WheelMovie wm ON m.Id = wm.MovieId
        //               WHERE wm.WheelId = @WheelId";
        // var param = new { WheelId = wheelId };
        // return await connection.QueryAsync<Models.Movie>(query, param);
        throw new NotImplementedException();
    }

    public async Task SetEliminatedAsync(int wheelId, int movieId, bool eliminated)
    {
        throw new NotImplementedException();
    }

    public async Task SetWatchedDateAsync(int wheelId, int movieId, DateOnly? watchedDate)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Models.Movie>> GetRemainingByWheelAsync(int wheelId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Models.Movie>> GetLastWatchedAsync(int wheelId, int count)
    {
        throw new NotImplementedException();
    }

}
