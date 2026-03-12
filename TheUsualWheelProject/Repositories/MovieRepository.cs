using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Repositories;

public class MovieRepository : IMovieRepository
{
    public MovieRepository(string conString) : base(TableName:"Movie", int, conString)
    {
        
    }
    // This is where you would implement methods to interact with your data source (e.g., database, API)
    // For example:
    // public Task<List<Wheel>> GetWheelsAsync() { ... }
    // public Task AddWheelAsync(Wheel wheel) { ... }
}
