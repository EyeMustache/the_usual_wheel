using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject.Services;

public class WheelService
{
    private readonly IWheelRepository _wheelRepository;
    private readonly IMovieRepository _movieRepository;

    public WheelService(IWheelRepository wheelRepository, IMovieRepository movieRepository)
    {
        _wheelRepository = wheelRepository;
        _movieRepository = movieRepository;
    }
}