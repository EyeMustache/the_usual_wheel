using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Models;
using TMDbLib.Objects.Movies;

namespace TheUsualWheelProject.Repositories.Interfaces;

public interface IMovieRepository : IGenericRepository<Models.Movie, int>
{

}