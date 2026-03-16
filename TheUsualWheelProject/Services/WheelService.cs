using TheUsualWheelProject.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace TheUsualWheelProject.Services;

public class WheelService : LoggingBase<WheelService>
{
    private readonly IWheelRepository _wheelRepository;
    private readonly IMovieRepository _movieRepository;

    public WheelService(IWheelRepository wheelRepository, IMovieRepository movieRepository, ILogger<WheelService> logger) : base(logger)
    {
        _wheelRepository = wheelRepository;
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<Models.Wheel>> GetAllWheelsAsync()
    {
        Logger.LogInformation("Fetching all wheels.");
        IEnumerable<Models.Wheel> wheels = await _wheelRepository.GetAll();
        // debug log
        foreach (var wheel in wheels)
        {
            Logger.LogDebug("Fetched wheel with ID {wheelId} and name {wheelName}.", wheel.Id, wheel.Name);
        }
        return wheels;
    }

    public async Task<Models.Wheel?> GetWheelByIdAsync(int id)
    {
        Logger.LogInformation("Fetching wheel by ID:{id}.", id);
        return await _wheelRepository.GetById(id);
    }

    public async Task<Models.Wheel?> AddWheelAsync(Models.Wheel wheel)
    {
        Logger.LogInformation("Adding new wheel:{wheel.Id}.", wheel.Id);
        await _wheelRepository.Insert(wheel);
        return await _wheelRepository.GetById(wheel.Id);
    }

    public async Task UpdateWheelAsync(Models.Wheel wheel)
    {
        Logger.LogInformation("Updating wheel with ID {wheel.Id}.", wheel.Id);
        var existingWheel = await _wheelRepository.GetById(wheel.Id);
        if (existingWheel == null)
        {
            
            Logger.LogWarning("Wheel with ID {wheel.Id} does not exist.", wheel.Id);
            throw new InvalidOperationException("Wheel does not exist.");
        }


        if (wheel.Id != existingWheel.Id)
        {
            Logger.LogWarning("Wheel ID mismatch: provided ID {wheel.Id} does not match existing ID {existingWheel.Id}.", wheel.Id, existingWheel.Id);
            throw new InvalidOperationException("Wheel ID mismatch.");
        }

        await _wheelRepository.Update(wheel);
    }

    public async Task DeleteWheelAsync(int id)
    {
        Logger.LogInformation("Deleting wheel with ID {id}.", id);
        var wheel = await _wheelRepository.GetById(id);
        if (wheel == null)
        {
            Logger.LogWarning("Wheel with ID {id} does not exist.", id);    
            throw new InvalidOperationException("Wheel does not exist.");
        }

        await _wheelRepository.Delete(id);
    }

    public async Task AddMovieToWheelAsync(int wheelId, int movieId)
    {
        Logger.LogInformation("Adding movie with ID {movieId} to wheel with ID {wheelId}.", movieId, wheelId);
        var wheel = await _wheelRepository.GetById(wheelId);
        if (wheel == null)
            throw new InvalidOperationException("Wheel does not exist.");
    
        var movie = await _movieRepository.GetById(movieId);
        if (movie == null)
            throw new InvalidOperationException("Movie does not exist.");
    
        await _wheelRepository.AddMovieToWheelAsync(wheelId, movieId);
    }
    public async Task<IEnumerable<Models.WheelMovie>> GetWheelMoviesAsync(int wheelId)
    {
        Logger.LogInformation("Fetching wheel movies for wheel ID {wheelId}.", wheelId);
        return await _wheelRepository.GetMoviesByWheelIdAsync(wheelId);
    }
}