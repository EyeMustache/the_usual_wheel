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

    public async Task<IEnumerable<Models.Wheel>> GetAllWheelsAsync()
    {
        return await _wheelRepository.GetAll();
    }

    public async Task<Models.Wheel?> GetWheelByIdAsync(int id)
    {
        return await _wheelRepository.GetById(id);
    }

    public async Task AddWheelAsync(Models.Wheel wheel)
    {
        await _wheelRepository.Insert(wheel);
    }

    public async Task UpdateWheelAsync(Models.Wheel wheel)
    {
        var existingWheel = await _wheelRepository.GetById(wheel.Id);
        if (existingWheel == null)
            throw new InvalidOperationException("Wheel does not exist.");

        if (wheel.Id != existingWheel.Id)
            throw new InvalidOperationException("Wheel ID mismatch.");

        await _wheelRepository.Update(wheel);
    }

    public async Task DeleteWheelAsync(int id)
    {
        var wheel = await _wheelRepository.GetById(id);
        if (wheel == null)
            throw new InvalidOperationException("Wheel does not exist.");

        await _wheelRepository.Delete(id);
    }  
}