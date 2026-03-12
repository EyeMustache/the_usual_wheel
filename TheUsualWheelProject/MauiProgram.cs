using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TheUsualWheelProject.Repositories;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Services;
using Plugin.Maui.Audio;
using Dapper;

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
		// Add dependencies
		builder.UseSkiaSharp();
		builder.Services.AddSingleton<IMovieRepository>(_ => new MovieRepository(DatabaseConfig.ConnectionString));
		builder.Services.AddSingleton<IWheelRepository>(_ => new WheelRepository(DatabaseConfig.ConnectionString));
		builder.Services.AddSingleton<MovieService>();
		builder.Services.AddSingleton<WheelService>();
		builder.Services.AddSingleton<TmdbService>();
		builder.Services.AddSingleton(AudioManager.Current);
		builder.Services.AddSingleton<AudioService>();
		builder.Services.AddSingleton<DatabaseService>(
			new DatabaseService(DatabaseConfig.ConnectionString));

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddTransient<MainPage>();

		return builder.Build();
	}
}
