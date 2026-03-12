using TheUsualWheelProject.Views;

namespace TheUsualWheelProject;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(WheelListPage), typeof(Views.WheelListPage));
    }
}
