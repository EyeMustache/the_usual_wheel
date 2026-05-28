using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Repositories;

public class BreakRepository : GenericRepository<Break, int>, IBreakRepository
{
    public BreakRepository(string conString) : base("MovieBreak", conString) { }

    public async Task<IEnumerable<Break>> GetBreaksByMovieIdAsync(int movieId)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        var query = @"
            SELECT * FROM MovieBreak
            WHERE MovieId = @MovieId
            ORDER BY BreakNumber ASC";
            
        return await connection.QueryAsync<Break>(query, new { MovieId = movieId });
    }

    public async Task InsertBreaksAsync(IEnumerable<Break> breaks)
    {
        using var connection = new SqliteConnection(_conString);
        await connection.OpenAsync();
        
        var query = @"
            INSERT OR REPLACE INTO MovieBreak (MovieId, BreakNumber, Timestamp, VibeCheck, Recap)
            VALUES (@MovieId, @BreakNumber, @Timestamp, @VibeCheck, @Recap)";
            
        await connection.ExecuteAsync(query, breaks);
    }
}