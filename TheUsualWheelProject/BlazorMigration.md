# Plan: Migrate .NET MAUI XAML to Blazor Hybrid

This plan will guide you through converting your project to use Blazor for the UI, allowing you to leverage your web development skills with HTML, CSS, and C#. We will convert the `WheelListPage` as the primary example.

---

### Phase 1: Project Configuration

**1. Modify the Project File (`.csproj`)**
- Open `TheUsualWheelProject.csproj`.
- Inside the first `<PropertyGroup>`, add this line to enable Razor component compilation:
    ```xml
    <EnableDefaultCssItems>false</EnableDefaultCssItems>
    ```
- This prevents conflicts with the default MAUI styling system.

**2. Update `MauiProgram.cs`**
- Add the necessary Blazor services.
- In `MauiProgram.cs`, chain these two methods to your `builder`:
    ```csharp
    // In MauiProgram.cs, inside CreateMauiApp()
    builder
        .UseMauiApp<App>()
        // ... existing builder configuration
        .UseSkiaSharp(); // This is the last line from your file
    
    // Add these lines for Blazor Hybrid
    builder.Services.AddMauiBlazorWebView();
    #if DEBUG
    builder.Services.AddBlazorWebViewDeveloperTools();
    builder.Logging.AddDebug(); // This might already be here
    #endif
    ```

---

### Phase 2: Create the Blazor UI Structure

**3. Create `wwwroot` and `index.html`**
- In the root of your project, create a new folder named `wwwroot`.
- Inside `wwwroot`, create a new file named `index.html`. This file will be the host page for your Blazor app.
- Add the following content to `index.html`:
    ```html
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>The Usual Wheel</title>
        <base href="/" />
        <link href="css/app.css" rel="stylesheet" />
    </head>
    <body>
        <div id="app">Loading...</div>
        <script src="_framework/blazor.webview.js"></script>
    </body>
    </html>
    ```

**4. Add CSS File**
- Inside `wwwroot`, create a new folder named `css`.
- Inside `wwwroot/css`, create a file named `app.css`. You can leave it empty for now or add some basic styles:
    ```css
    html, body {
        font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
    }
    ```

**5. Create `_Imports.razor`**
- In the root of your project, create a new file named `_Imports.razor`.
- This file acts like global `using` statements for all your Razor components.
- Add the following content:
    ```razor
    @using System.Net.Http
    @using Microsoft.AspNetCore.Components.Forms
    @using Microsoft.AspNetCore.Components.Routing
    @using Microsoft.AspNetCore.Components.Web
    @using Microsoft.JSInterop
    @using TheUsualWheelProject
    @using TheUsualWheelProject.Models
    @using TheUsualWheelProject.ViewModels
    ```

---

### Phase 3: Convert the UI

**6. Create the Main Blazor Layout**
- Delete `AppShell.xaml` and `AppShell.xaml.cs`.
- In the root of your project, create a new file named `Main.razor`. This will be the root component of your Blazor UI.
- Add the following content:
    ```razor
    <Router AppAssembly="@typeof(Main).Assembly">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" />
        </Found>
        <NotFound>
            <LayoutView>
                <p>Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
    ```

**7. Replace `AppShell` with `BlazorWebView`**
- Open `App.xaml.cs`.
- Change the `MainPage` assignment in the constructor to use a `BlazorWebView`.
- Your `App.xaml.cs` should look like this:
    ```csharp
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // The MainPage is now a ContentPage holding the Blazor UI
            MainPage = new ContentPage
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
        }
    }
    ```
- **Note:** You can delete the `CreateWindow` override method if it's still there.

**8. Convert `WheelListPage` to Blazor**
- Create a new folder named `Pages`.
- Inside `Pages`, create a new file named `WheelListPage.razor`.
- This component will replace `WheelListPage.xaml`. Add the following code:
    ```razor
    @page "/wheels"
    @inject WheelListViewModel ViewModel

    <h1>Wheels</h1>

    @if (ViewModel.Wheels == null || !ViewModel.Wheels.Any())
    {
        <p><em>Loading or no wheels found...</em></p>
    }
    else
    {
        <ul>
            @foreach (var wheel in ViewModel.Wheels)
            {
                <li>@wheel.Name - @wheel.Description</li>
            }
        </ul>
    }

    @code {
        protected override async Task OnInitializedAsync()
        {
            // This replaces the OnAppearing logic
            await ViewModel.LoadWheelsCommand.ExecuteAsync(null);
        }
    }
    ```

---

### Phase 4: Final Cleanup and Verification

**9. Clean Up Old Files**
- You can now safely delete the following XAML-related files:
    - `Views/WheelListPage.xaml`
    - `Views/WheelListPage.xaml.cs`
    - `Views/WheelPage.xaml`
    - `Views/WheelPage.xaml.cs`
    - `MainPage.xaml`
    - `MainPage.xaml.cs`

**10. Set the Default Route**
- To make the `WheelListPage` the default page, you can add a `@page "/"` directive to the top of `Pages/WheelListPage.razor` alongside the existing `@page "/wheels"`.

---

### Verification

1.  Run `dotnet build` to ensure there are no compilation errors.
2.  Run the app on your Android emulator (`dotnet build -t:Run -f net9.0-android`).
3.  The app should launch and display a page with the title "Wheels" and a list of the wheels from your database, rendered with HTML.
