using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Services;

namespace TheUsualWheelProject;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}