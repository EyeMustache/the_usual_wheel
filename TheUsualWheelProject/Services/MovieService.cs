using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Services;

public class MovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }
}