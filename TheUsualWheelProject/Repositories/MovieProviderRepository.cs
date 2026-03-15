using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Repositories;

public class MovieProviderRepository : GenericRepository<MovieProvider, int>, IMovieProviderRepository
{
    public MovieProviderRepository(string conString) : base("MovieProvider", conString) { }

    public async Task<IEnumerable<WatchProvider>> GetProvidersByMovieIdAsync(int movieId)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        var query = @"
            SELECT p.*
            FROM WatchProvider p
            INNER JOIN MovieProvider mp ON p.ProviderId = mp.ProviderId
            WHERE mp.MovieId = @MovieId
            ORDER BY p.DisplayPriority ASC, p.Name ASC";

        return await connection.QueryAsync<WatchProvider>(query, new { MovieId = movieId });
    }

    public async Task LinkProviderToMovieAsync(int movieId, int providerId, string type)
    {
        // Debug line
        Console.WriteLine($"Linking provider ID {providerId} to movie ID {movieId} with type '{type}'...");
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        var query = @"
            INSERT OR IGNORE INTO MovieProvider (MovieId, ProviderId, Type)
            VALUES (@MovieId, @ProviderId, @Type)";

        await connection.ExecuteAsync(query, new { MovieId = movieId, ProviderId = providerId, Type = type });
    }

    public async Task ClearProvidersForMovieAsync(int movieId)
    {   
        // Debug line
        Console.WriteLine($"Clearing providers for movie ID {movieId}...");
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        var query = "DELETE FROM MovieProvider WHERE MovieId = @MovieId";
        await connection.ExecuteAsync(query, new { MovieId = movieId });
    }
}
