using TheUsualWheelProject.Models;
using TheUsualWheelProject.Repositories.Interfaces;
using TheUsualWheelProject.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace TheUsualWheelProject;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var contentPage = new ContentPage
        {
            Content = new BlazorWebView
            {
                HostPage = "wwwroot/index.html",
                RootComponents =
                {
                    new RootComponent
                    {
                        Selector = "#app",
                        ComponentType = typeof(Main)
                    }
                }
            }
        };

        return new Window(contentPage);
    }
}