using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            var serviceProvider = IPlatformApplication.Current!.Services;
            var wheelRepo = serviceProvider.GetRequiredService<IWheelRepository>();
            var movieRepo = serviceProvider.GetRequiredService<IMovieRepository>();
            var dbService = serviceProvider.GetRequiredService<DatabaseService>();

            dbService.InitAsync().GetAwaiter().GetResult();
            dbService.ClearAllDataAsync().GetAwaiter().GetResult();
            var wheel = new Wheel { Name = "Russell Crowe" };
            wheelRepo.Insert(wheel).GetAwaiter().GetResult();

            var gladiator = new Movie { Title = "Gladiator", Year = 2000, TmdbId = 98, Genre = "Action", Director = "Ridley Scott" };
            var masterAndCommander = new Movie { Title = "Master and Commander", Year = 2003, TmdbId = 1778, Genre = "Adventure", Director = "Peter Weir" };

            movieRepo.Insert(gladiator).GetAwaiter().GetResult();
            movieRepo.Insert(masterAndCommander).GetAwaiter().GetResult();

            var allMovies = movieRepo.GetAll().GetAwaiter().GetResult();
            var wheels = wheelRepo.GetAll().GetAwaiter().GetResult();
            System.Diagnostics.Debug.WriteLine($"Wheels: {string.Join(", ", wheels.Select(w => w.Name))}");
            System.Diagnostics.Debug.WriteLine($"Movies: {string.Join(", ", allMovies.Select(m => m.Title))}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SMOKE TEST ERROR: {ex}");
            MainPage = new ContentPage
            {
                Content = new Label { Text = $"SMOKE TEST ERROR: {ex.Message}" }
            };
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}