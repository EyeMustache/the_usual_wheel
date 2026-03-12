using Dapper;
using Microsoft.Data.Sqlite;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TMDbLib.Objects.Movies;

namespace TheUsualWheelProject.Repositories;

public class MovieRepository : GenericRepository<Models.Movie, int>, IMovieRepository
{
    public MovieRepository(string conString) : base("Movie", conString)
    {
        throw new NotImplementedException();
    }

}
