using TheUsualWheelProject.ViewModels;

namespace TheUsualWheelProject.Views;

public partial class WheelListPage : ContentPage
{
    public WheelListPage(WheelListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}