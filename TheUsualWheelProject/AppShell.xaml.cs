namespace TheUsualWheelProject;

public partial class AppShell : Shell
{
    public AppShell()
    {
        var mainPage = IPlatformApplication.Current!.Services.GetRequiredService<MainPage>();
        Items.Add(new ShellContent
        {
            Title = "Home",
            Content = mainPage,
            Route = "MainPage"
        });
    }
}
