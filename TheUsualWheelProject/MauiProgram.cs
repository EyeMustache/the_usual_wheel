using Microsoft.Extensions.Logging;
using TheUsualWheelProject.Repositories;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Services;
using TheUsualWheelProject.ViewModels;
using TheUsualWheelProject.Models;
using TheUsualWheelProject.Components;
using Plugin.Maui.Audio;
using Dapper;
using Microsoft.AspNetCore.Components.WebView.Maui;
using MudBlazor.Services;
using MudBlazor;

namespace TheUsualWheelProject;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		
		SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
		SqlMapper.AddTypeHandler(new WheelConfigTypeHandler());

		// Repositories
        builder.Services.AddSingleton<IGenericRepository<Movie, int>>(_ => new GenericRepository<Movie, int>("Movies", DatabaseConfig.ConnectionString));
        builder.Services.AddSingleton<IGenericRepository<Wheel, int>>(_ => new GenericRepository<Wheel, int>("Wheels", DatabaseConfig.ConnectionString));
        builder.Services.AddSingleton<IMovieRepository>(_ => new MovieRepository(DatabaseConfig.ConnectionString));
        builder.Services.AddSingleton<IWheelRepository>(_ => new WheelRepository(DatabaseConfig.ConnectionString));
		builder.Services.AddSingleton<IWatchProviderRepository>(_ => new WatchProviderRepository(DatabaseConfig.ConnectionString));
		builder.Services.AddSingleton<IMovieProviderRepository>(_ => new MovieProviderRepository(DatabaseConfig.ConnectionString));
		builder.Services.AddSingleton<IBreakRepository>(_ => new BreakRepository(DatabaseConfig.ConnectionString));
		// Services
        builder.Services.AddSingleton<MovieService>();
        builder.Services.AddSingleton<WheelService>();
        builder.Services.AddSingleton<TmdbService>();
		builder.Services.AddSingleton<WatchProviderService>();
        builder.Services.AddSingleton<DatabaseService>(
			new DatabaseService(DatabaseConfig.ConnectionString, new WheelRepository(DatabaseConfig.ConnectionString), new MovieRepository(DatabaseConfig.ConnectionString), new LoggerFactory().CreateLogger<DatabaseService>()));
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<AudioService>();
		builder.Services.AddHttpClient<BreakService>();
		// ViewModels
		builder.Services.AddSingleton<MainPageViewModel>();
		builder.Services.AddSingleton<WheelViewModel>();
		builder.Services.AddSingleton<MovieViewModel>();
		builder.Services.AddSingleton<WheelListViewModel>();
		builder.Services.AddSingleton<MovieSearchViewModel>();
		builder.Services.AddSingleton<MovieDetailsViewModel>();
        
        //builder.UseSkiaSharp();
        builder.Services.AddMauiBlazorWebView();

		builder.Services.AddMudServices(config =>
		{
			config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
			config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		builder.Logging.AddConsole(); 
		builder.Logging.SetMinimumLevel(LogLevel.Information);

		var app = builder.Build();


		var loggerFactory = app.Services.GetService<ILoggerFactory>();
		var logger = loggerFactory?.CreateLogger("MauiProgram");

		// Startup log
		logger?.LogInformation("App startup sequence initiated");

		// Log DI registrations
		logger?.LogInformation("Registering repositories and services...");
		
		logger?.LogInformation("DI registration complete.");

		// Log SkiaSharp and Blazor setup
		logger?.LogInformation("SkiaSharp and BlazorWebView configured.");

		// Final startup log
		logger?.LogInformation("App startup sequence complete.");

		var db = app.Services.GetRequiredService<DatabaseService>();
		
		return app;
	}
}
