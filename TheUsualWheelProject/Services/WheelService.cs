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
        return await _wheelRepository.GetAll();
    }

    public async Task<Models.Wheel?> GetWheelByIdAsync(int id)
    {
        Logger.LogInformation("Fetching wheel by ID:{id}.", id);
        return await _wheelRepository.GetById(id);
    }

    public async Task AddWheelAsync(Models.Wheel wheel)
    {
        Logger.LogInformation("Adding new wheel:{wheel.Id}.", wheel.Id);
        await _wheelRepository.Insert(wheel);
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
}