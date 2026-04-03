using TheUsualWheelProject.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using TheUsualWheelProject.Models;

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
            Logger.LogDebug($"Fetched wheel with ID {wheel.Id} and name {wheel.Name}.");
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
        Logger.LogInformation($"Adding new wheel:{wheel.Id}.");
        await _wheelRepository.Insert(wheel);
        return await _wheelRepository.GetById(wheel.Id);
    }

    public async Task UpdateWheelAsync(Models.Wheel wheel)
    {
        Logger.LogInformation($"Updating wheel with ID {wheel.Id}.");
        Models.Wheel? existingWheel = await _wheelRepository.GetById(wheel.Id);
        if (wheel.CompareTo(existingWheel) == 0)
        {
            Logger.LogInformation($"No changes detected for wheel with ID {wheel.Id}. Skipping update.");
            return;
        }
        else if (wheel.CompareTo(existingWheel) != 0 && existingWheel != null)
        {
            Logger.LogInformation($"Changes detected for wheel with ID {wheel.Id}. Proceeding with update.");
            await _wheelRepository.Update(wheel);
        }
        else
        {
            Logger.LogWarning($"Wheel with ID {wheel.Id} does not exist.");
            throw new InvalidOperationException("Wheel does not exist.");
        }
    }

    public async Task DeleteWheelAsync(int id)
    {
        Logger.LogInformation($"Deleting wheel with ID {id}.");
        var wheel = await _wheelRepository.GetById(id);
        if (wheel == null)
        {
            Logger.LogWarning($"Wheel with ID {id} does not exist.");
            throw new InvalidOperationException("Wheel does not exist.");
        }

        await _wheelRepository.Delete(id);
    }

    public async Task AddMovieToWheelAsync(int movieId, int wheelId)
    {
        Logger.LogInformation($"Adding movie with ID {movieId} to wheel with ID {wheelId}.");
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
        Logger.LogInformation($"Fetching wheel movies for wheel ID {wheelId}.");
        return await _wheelRepository.GetMoviesByWheelIdAsync(wheelId);
    }

    public async Task UpdateMovieEliminationStatusAsync(int wheelId, int movieId, bool isEliminated)
    {
        Logger.LogInformation($"Updating elimination status for movie {movieId} in wheel {wheelId} to {isEliminated}.");
        if (isEliminated)
        {
            WheelMovie wheelMovie = new WheelMovie
            {
                WheelId = wheelId,
                MovieId = movieId,
                IsEliminated = true,
                WatchedDate = DateOnly.FromDateTime(DateTime.Now)
            };
        }
        await _wheelRepository.UpdateMovieEliminationStatusAsync(wheelId, movieId, isEliminated);
    }

    public async Task SetMovieWatchedStatusAsync(int wheelMovieId, bool isWatched)
    {
        Logger.LogInformation($"Setting watched status for wheelMovieId {wheelMovieId} to {isWatched}.");
        DateOnly? watchedDate = isWatched ? DateOnly.FromDateTime(DateTime.Now) : null;
        await _wheelRepository.UpdateMovieWatchedDateAsync(wheelMovieId, watchedDate);
    }

    public async Task RemoveMovieFromWheelAsync(int movieId, int wheelId)
    {
        Logger.LogInformation($"Removing movie with ID {movieId} from wheel with ID {wheelId}.");
        await _wheelRepository.RemoveMovieFromWheelAsync(wheelId, movieId);
    }

    public async Task RemoveMoviesFromWheelAsync(IEnumerable<int> wheelMovieIds)
    {
        Logger.LogInformation($"Removing movies with IDs {string.Join(", ", wheelMovieIds)} from their respective wheels.");
        await _wheelRepository.RemovieMoviesFromWheelAsync(wheelMovieIds);
    }
}