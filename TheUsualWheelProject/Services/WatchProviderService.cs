using Microsoft.Extensions.Logging;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TmdbWatchProviderItem = TMDbLib.Objects.General.WatchProviderItem;

namespace TheUsualWheelProject.Services;

public class WatchProviderService : LoggingBase<WatchProviderService>
{
    private readonly IWatchProviderRepository _watchProviderRepository;
    private readonly IMovieProviderRepository _movieProviderRepository;

    public WatchProviderService(
        IWatchProviderRepository watchProviderRepository,
        IMovieProviderRepository movieProviderRepository,
        ILogger<WatchProviderService> logger) : base(logger)
    {
        _watchProviderRepository = watchProviderRepository;
        _movieProviderRepository = movieProviderRepository;
    }

    public async Task<IEnumerable<WatchProvider>> GetProvidersByMovieAsync(int movieId)
    {
        Logger.LogInformation("Fetching watch providers for movie ID {movieId}.", movieId);
        return await _movieProviderRepository.GetProvidersByMovieIdAsync(movieId);
    }

    public async Task ClearProvidersForMovieAsync(int movieId)
    {
        Logger.LogInformation("Clearing watch providers for movie ID {movieId}.", movieId);
        await _movieProviderRepository.ClearProvidersForMovieAsync(movieId);
    }

    public async Task UpdateMovieProvidersAsync(int movieId, IEnumerable<TmdbWatchProviderItem> providers, string type)
    {
        var providerList = providers?.ToList() ?? [];
        Logger.LogInformation("Updating watch providers for movie ID {movieId} and type {type} with {providerCount} providers.", movieId, type, providerList.Count);

        var skippedMissingProviderId = 0;
        var linkedCount = 0;

        foreach (var provider in providerList)
        {
            if (!provider.ProviderId.HasValue)
            {
                skippedMissingProviderId++;
                continue;
            }

            var providerId = provider.ProviderId.Value;
            var logoPath = provider.LogoPath ?? string.Empty;
            var logoUrl = logoPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? logoPath
                : $"https://image.tmdb.org/t/p/w92{logoPath}";

            var watchProvider = new WatchProvider
            {
                ProviderId = providerId,
                Name = provider.ProviderName ?? "Unknown",
                LogoUrl = logoUrl,
                DisplayPriority = provider.DisplayPriority ?? 0
            };

            await _watchProviderRepository.UpsertAsync(watchProvider);

            await _movieProviderRepository.LinkProviderToMovieAsync(movieId, providerId, type);
            linkedCount++;
        }

        Logger.LogInformation(
            "Finished updating movie ID {movieId} and type {type}: linked={linkedCount}, skippedWithoutProviderId={skippedCount}.",
            movieId,
            type,
            linkedCount,
            skippedMissingProviderId);
    }
}
