using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using TheUsualWheelProject.Repositories;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Services;
using Plugin.Maui.Audio;


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
		
		// Add dependencies
		builder.UseSkiaSharp();
		builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
   		builder.Services.AddSingleton<IWheelRepository, WheelRepository>();
   		builder.Services.AddSingleton<MovieService>();
   		builder.Services.AddSingleton<WheelService>();
   		builder.Services.AddSingleton<TmdbService>();
   		builder.Services.AddSingleton<MovieService>();
   		builder.Services.AddSingleton(AudioManager.Current);
		builder.Services.AddSingleton<AudioService>();


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
