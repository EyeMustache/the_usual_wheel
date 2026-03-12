using Microsoft.Maui.Controls;
using TheUsualWheelProject.Repositories.Interfaces;

namespace TheUsualWheelProject
{
    public partial class MainPage : ContentPage
    {
        public MainPage(IWheelRepository wheelRepo, IMovieRepository movieRepo)
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel(wheelRepo, movieRepo);
        }
    }
}