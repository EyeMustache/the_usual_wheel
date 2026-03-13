# The Usual Wheel — Developer Guide

#### Quick Reference

1/100 kans dat de wheel spin een scheet geluid maakt lol

ADD STREAMING AVAILABLE SERVICES 
To run:
dotnet build -t:Run -f net9.0-android

## Context

This app was built for personal use by two friends who spin a wheel of movies starring a
specific actor, smoke some weed, and watch whatever the wheel lands on. It started with
Russell Crowe, and will eventually move on to new actors like Denzel Washington once a wheel is fully completed.

They rotate hosting between each other's places and take smoke breaks outside, so they
want pre-planned break timestamps for each movie, a timestamp, a "vibe check" describing
where exactly to pause, and a recap for when they come back in.

The wheel eliminates movies one by one. The last movie standing is the one that gets
watched. Once a whole wheel is done (e.g. all Russell Crowe movies are watched), a new
wheel starts fresh (e.g. Denzel Washington).

**When using AI assistance, share this file for context. It contains all decisions made.**

---

## Decisions Made

| Decision | Choice | Reason |
|---|---|---|
| Framework | .NET MAUI Blazor Hybrid | Android + iOS from one C# codebase using web tech (HTML/CSS) for UI |
| Language | C# / HTML / CSS / Razor | Learning-focused, fits existing web-dev skills |
| Architecture | MVVM + three-layer (Presentation → Business → Data) | Clean separation, models/services mapped perfectly to Blazor |
| Database | SQLite via `Microsoft.Data.Sqlite` + `Dapper` | Familiar manual SQL + Dapper pattern, full join support |
| Smoke Break Data | JSON files (one per movie) | Hand-editable, easy to paste output from Gemini |
| Movie Metadata | TMDB API via `TMDbLib` | Free, rich data, easy NuGet package |
| Ratings | TMDB rating auto-fetched + optional manual Letterboxd override | Letterboxd has no public API |
| Wheel Drawing | SkiaSharp (`SkiaSharp.Views.Maui.Controls`) | Custom 2D canvas, full control |
| Audio | Bundled MP3 via `Plugin.Maui.Audio` | Price is Right music during wheel spin |
| MVVM Toolkit | `CommunityToolkit.Mvvm` | Microsoft-maintained, modern, minimal boilerplate |
| Break Entry | External JSON files, imported into Resources/Raw | Easy to write manually or paste from Gemini |
| Deployment | Sideloaded APK | Personal use only for now |
| Gemini Integration | Not in MVP | Planned future feature |
| Host Tracking | Not included | Handled informally between friends |
| Testing | MSTest + Moq, separate test project | Catch regressions as the codebase grows |

---

## MVP Scope

### In scope

- Multiple named wheels (e.g. "Russell Crowe", "Denzel Washington")
- Classic spinning wheel visual with animated spin and audio
- Movie elimination — selected movies are greyed out and set aside until last one standing
- Movie details page showing poster, title, synopsis, director, duration, cast, and rating
- Smoke break display loaded from per-movie JSON files (optional tick)
- TMDB movie search when adding a new movie to a wheel (or manual import)
- Optional manual Letterboxd rating field per movie
- Reset wheel with double confirmation

### Out of scope (for now)

- Basic stats per wheel: total movies, remaining, watched, last played list
- Gemini in-app integration (future phase)
- Google Play deployment
- Host/location turn tracking
- Rating-based status for movies

---

## Project Structure

```
TheUsualWheelProject/
├── MauiProgram.cs
├── App.xaml / App.xaml.cs
├── Main.razor (Blazor root)
├── _Imports.razor
├── wwwroot/
│   ├── index.html
│   └── css/app.css
├── Models/
│   ├── Movie.cs
│   ├── Wheel.cs
│   ├── WheelMovies.cs
│   └── SmokeBreak.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IMovieRepository.cs
│   │   └── IWheelRepository.cs
│   ├── MovieRepository.cs
│   └── WheelRepository.cs
├── Services/
│   ├── MovieService.cs
│   ├── WheelService.cs
│   ├── TmdbService.cs
│   └── JsonDataService.cs
├── ViewModels/
│   ├── WheelListViewModel.cs
│   ├── WheelViewModel.cs
│   ├── MovieListViewModel.cs
│   └── MovieDetailsViewModel.cs
├── Pages/
│   ├── WheelListPage.razor
│   ├── WheelPage.razor
│   ├── MovieListPage.razor
│   └── MovieDetailsPage.razor
├── Controls/
│   └── WheelControl.cs
└── Resources/
    └── Raw/
        ├── price_is_right.mp3
        ├── breaks_gladiator.json
        └── breaks_master-and-commander.json
```

---

## Data Schemas

### SQLite Tables

```
Wheels
  Id              INTEGER  PRIMARY KEY AUTOINCREMENT
  Name            TEXT     e.g. "Russell Crowe"
  Description     TEXT

Movies
  Id              INTEGER  PRIMARY KEY AUTOINCREMENT
  Title           TEXT
  Year            INTEGER
  Director        TEXT
  DurationMinutes INTEGER
  Synopsis        TEXT
  PosterUrl       TEXT
  TmdbRating      REAL
  LetterboxdRating REAL    nullable — entered manually
  TmdbId          INTEGER

WheelMovies                              ← junction table (many-to-many)
  Id              INTEGER  PRIMARY KEY AUTOINCREMENT
  WheelId         INTEGER  FOREIGN KEY → Wheels.Id
  MovieId         INTEGER  FOREIGN KEY → Movies.Id
  IsEliminated    INTEGER  0 = active, 1 = out — per wheel entry, not per movie
  WatchedDate     TEXT     nullable — ISO 8601 date, per wheel entry

Note: IsEliminated and WatchedDate live on WheelMovies, not Movies, because
the same film (e.g. American Gangster) can exist on multiple wheels independently.
```

### Smoke Break JSON Schema

One file per movie, stored in `Resources/Raw/`.
Filename convention: `breaks_{slug}.json` where slug is lowercase and hyphen-separated.

```json
{
  "movieTitle": "Gladiator",
  "tmdbId": 98,
  "breaks": [
    {
      "number": 1,
      "timestamp": "00:42:00",
      "vibeCheck": "Maximus has just arrived at the gladiatorial school. Pause on the wide shot of him staring at the cell door after it closes.",
      "recap": "Maximus has gone from Roman general to slave. He knows Commodus ordered the murder of his wife and son, and he is beginning to find a reason to stay alive."
    },
    {
      "number": 2,
      "timestamp": "01:18:00",
      "vibeCheck": "The Colosseum crowd has gone quiet after Maximus revealed his identity. Pause as the camera slowly pulls back on Commodus frozen in his box.",
      "recap": "Maximus is now a celebrity fighter and Commodus knows exactly who he is. The political tension in Rome is about to shift entirely."
    }
  ]
}
```

---

## Phase-by-Phase Guide

> This is a learning project. The goal is to understand what you are building, not just
> get it working. Read through the linked documentation before coding each phase.
> Look things up, experiment, and ask questions when something does not make sense.

---

### Phase 1 — Project Setup

**Goal:** A running MAUI Blazor Hybrid app shell with all dependencies wired up.

**Steps:**

1. The project was initially created as a MAUI app but has been converted to use the **MAUI Blazor Hybrid** model.
2. Ensure you have the `Microsoft.AspNetCore.Components.WebView.Maui` NuGet package installed, and change the Sdk to `Microsoft.NET.Sdk.Razor` in the `.csproj`.
3. Open `MauiProgram.cs` and register the following:
   - `builder.Services.AddMauiBlazorWebView()`
   - `.UseSkiaSharp()` (SkiaSharp MAUI extension method)
   - `builder.Services.AddSingleton<IMovieRepository, MovieRepository>()`
   - `builder.Services.AddSingleton<IWheelRepository, WheelRepository>()`
   - `builder.Services.AddSingleton<MovieService>()`
   - `builder.Services.AddSingleton<WheelService>()`
   - `builder.Services.AddSingleton<TmdbService>()`
   - `builder.Services.AddSingleton<JsonDataService>()`
   - `builder.Services.AddSingleton(AudioManager.Current)` (Plugin.Maui.Audio)
4. Update `App.xaml.cs` to embed a `BlazorWebView` pointing to `wwwroot/index.html` and `typeof(Main)`.
5. Run the app on an Android emulator or physical device and confirm it launches to the Blazor loading screen.

**Key concept — Dependency Injection:**
MAUI uses the same DI system as ASP.NET Core. You register services once in
`MauiProgram.cs`. Your Blazor Pages (`.razor`) request them via `@inject` or through ViewModels, and the
framework handles wiring them up.

**Read:**
- https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui
- https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection

---

### Phase 2 — Data Models, Repositories & Database

**Goal:** Define your data structures and get SQLite reading and writing correctly
using Dapper and a generic repository pattern.

**Steps:**

1. Create a `DatabaseConfig.cs` (or similar) that holds the resolved DB file path:
   ```csharp
   public class DatabaseConfig
   {
       public static string DatabasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TheUsualWheel.db");
   }
   ```
2. Create `Services/DatabaseService.cs` with the following async methods:
   - `InitAsync()` — creates tables if they do not exist, called once at startup
   - `GetWheelsAsync()` — returns all wheels
   - `GetMoviesForWheelAsync(int wheelId)` — returns all movies for a given wheel
   - `SaveMovieAsync(Movie movie)` — inserts or updates a movie
   - `SaveWheelAsync(Wheel wheel)` — inserts or updates a wheel
   - `SetEliminatedAsync(int movieId, bool eliminated)` — marks a movie as out
   - `ResetWheelAsync(int wheelId)` — sets IsEliminated = false for all movies on a wheel
3. Call `InitAsync()` from `MauiProgram.cs` after building the app, or lazily on first use
   in `DatabaseService` itself.
4. Write a quick test: hardcode a `Wheel` and a `Movie`, save them, read them back,
   display the title in a label. Delete the test code once you are satisfied.

**Key concept — sqlite-net-pcl attributes:**
You decorate your model class with attributes and the library generates the SQL for you.

```csharp
[Table("Movies")]
public class Movie
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int WheelId { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsEliminated { get; set; }

    // etc.
}
```

There is no migration system here. If you change a model after the database has been
created, the simplest approach during development is to delete the app and let it
recreate the database fresh. For later production changes, look into `ALTER TABLE`.

**Read:**
- https://github.com/praeclarum/sqlite-net (the README is your main reference)

---

### Phase 3 — TMDB Integration

**Goal:** Search for a movie by title and auto-fill its metadata.

**Steps:**

1. Register for a free API key at https://www.themoviedb.org/settings/api
2. Store the key in a `Constants.cs` file. **Do not commit this file to GitHub.**
   Add `Constants.cs` to your `.gitignore`. Consider using a `Constants.template.cs`
   with placeholder values that you do commit, so future you knows the file is needed.
3. Create `Services/TmdbService.cs` wrapping `TMDbClient` from the `TMDbLib` package.
4. Implement `SearchMoviesAsync(string title)` — returns a list of search results with
   title, year, and TMDB ID so the user can choose the right one.
5. Implement `GetMovieDetailsAsync(int tmdbId)` — returns cast, director, duration,
   poster path, synopsis, and rating.
6. On your Add Movie screen: user types title → list of results appears → they tap one
   → fields fill in automatically → they can optionally add a Letterboxd rating → save.

**Key concept — search results:**
Searching "Gladiator" returns multiple results (the 2000 film, the 2023 sequel, etc.).
Always show the user a picker listing title and year so they select the right one
before saving. Never silently pick the first result.

**Read:**
- https://github.com/LordMike/TMDbLib
- https://developer.themoviedb.org/docs/getting-started

---

### Phase 4 — Smoke Break JSON

**Goal:** Write break data files and display them on the movie details page.

**Steps:**

1. Create your first break file following the schema above, e.g. `breaks_gladiator.json`.
2. Place it in `Resources/Raw/`. In Visual Studio, right-click the file → Properties and
   confirm the Build Action is set to `MauiAsset`.
3. Create `Services/JsonDataService.cs`. Use `FileSystem.OpenAppPackageFileAsync(filename)`
   to open a bundled file, then deserialize it with `System.Text.Json.JsonSerializer`.
4. Create `Models/SmokeBreak.cs` to match the JSON structure.
5. In `MovieDetailsViewModel`, call `JsonDataService` to load the breaks for the
   current movie (match by TMDB ID or filename slug) and expose them as a bindable list.

**Key concept — MauiAsset files:**
Files in `Resources/Raw/` are bundled into the APK at build time. They are read-only
at runtime. You access them via `FileSystem.OpenAppPackageFileAsync()`, not `File.Open()`.
This means to add a new movie's break file you add it to the project and rebuild.
That is intentional for this app since updates are infrequent.

**Read:**
- https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers
- https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview

---

### Phase 5 — The Wheel (SkiaSharp with Blazor)

**Goal:** A drawable, animated spinning wheel that selects a movie.

This is the most involved phase visually. SkiaSharp gives you a raw 2D canvas — you are
drawing every line, arc, and character yourself. In Blazor Hybrid, you use an `SKCanvasView` wrapper component.

**Steps:**

1. Use the `SkiaSharp.Views.Blazor` package (or interop with the MAUI `SKCanvasView`). 
2. Create `Controls/WheelControl.razor` (or `.cs` component) that renders the canvas.
3. On paint surface redraws:
   - Calculate canvas center (width / 2, height / 2) and radius.
   - Loop through your active movies and draw one arc segment per movie using `SKPath`.
   - Rotate the canvas context before drawing each segment's label so text sits upright
     inside its slice.
   - Use muted / greyed colours for eliminated movies.
4. Store a `float _rotationDegrees` field on the control. This is what changes during
   the animation.
5. Use Blazor timers or JSInterop `requestAnimationFrame` to drive your animation loop.
   Each tick: increment `_rotationDegrees` based on current speed, then trigger a canvas repaint.
6. Apply an easing function to slow the wheel down naturally. A simple approach:
   multiply the current speed by a decay factor (e.g. `speed *= 0.985f`) each tick.
   When speed drops below a threshold, stop the timer.
7. On stop, determine which segment is at the top (12 o'clock). 
8. Invoke an `EventCallback` that the parent Page subscribes to, passing the winning movie.

**Key SkiaSharp types to know:**
`SKCanvas`, `SKPaint`, `SKPath`, `SKCanvasView`, `SKColor`, `SKRect`, `SKMatrix`

**Read:**
- https://learn.microsoft.com/en-us/dotnet/api/skiasharp.views.blazor
- Arc drawing reference:
  https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/curves/arcs

---

### Phase 6 — Movie Details Page

**Goal:** A full movie info screen with smoke breaks and a scroll-to-reveal recap.

**Steps:**

1. Create `Pages/MovieDetailsPage.razor`.
2. `@inject MovieDetailsViewModel` which loads:
   - Movie data from `DatabaseService`
   - Break data from `JsonDataService`
3. Layout sections using HTML/CSS:
   - `<header>` with Poster + title
   - Metadata row (flexbox): director, year, duration, rating
   - `<p>` Synopsis block
   - Smoke Breaks section: `<ul>` listing each break with its number, timestamp, and vibe check
   - A spacer (`<div style="height: 100vh;"></div>`) with a label: **"↓ Scroll only after you are back inside ↓"**
   - Recaps section below: one recap per break, revealed only by scrolling past the spacer
4. Bind the image tag `<img src="@Movie.PosterUrl" />` directly to the TMDB poster URL string.
5. Use standard Blazor `@foreach` loops to render your collections.

**Read:**
- https://learn.microsoft.com/en-us/aspnet/core/blazor/components/data-binding
- https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling
- https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/
- https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/collectionview/

---

### Phase 7 — Stats & History

**Goal:** Useful per-wheel stats without overcomplicating things.

**Stats to show:**
- Total movies on this wheel
- Remaining (not yet eliminated)
- Watched (eliminated)
- Average runtime of remaining movies
- Last played list — most recent watches, sorted by `WatchedDate`

**Steps:**

1. Ensure `Movie.WatchedDate` is being set when a movie is eliminated (in
   `SetEliminatedAsync` or in `WheelViewModel` when the spin completes).
2. Add computed properties to `WheelViewModel` using LINQ:
   ```csharp
   public int TotalMovies => Movies.Count;
   public int RemainingCount => Movies.Count(m => !m.IsEliminated);
   public int WatchedCount => Movies.Count(m => m.IsEliminated);
   public double AverageRemainingMinutes =>
       Movies.Where(m => !m.IsEliminated).Average(m => m.DurationMinutes);
   ```
3. Add a stats summary section to the wheel page or a dedicated tab.
4. For the last played list, query movies ordered by `WatchedDate DESC` and take the
   most recent 5 or so.

---

### Phase 8 — Audio

**Goal:** Price is Right music plays while the wheel is spinning.

**Steps:**

1. Add your audio file (`.mp3` or `.wav`) to `Resources/Raw/` as a `MauiAsset`.
2. Register the audio service in `MauiProgram.cs`:
   ```csharp
   builder.Services.AddSingleton(AudioManager.Current);
   ```
3. Inject `IAudioManager` into `WheelViewModel` via constructor.
4. When the spin starts: create a player with `AudioManager.CreatePlayerAsync()`, then
   call `PlayAsync()`. Set looping if the plugin supports it; otherwise restart on
   completion.
5. When the spin stops: call `StopAsync()` and dispose the player.

**Read:**
- https://github.com/jfversluis/Plugin.Maui.Audio

---

## Testing

> The test project should be created early and left ready, even if it starts empty.
> The habit of writing a test when you add a feature (or when you fix a bug) is what
> makes it useful. You do not need to test everything — focus on business logic in
> Services and repository query correctness.

### Setup

1. Create a separate test project alongside your main project:
   ```
   dotnet new mstest -n TheUsualWheelProject.Tests
   ```
2. Add a project reference from the test project to the main project:
   ```
   dotnet add TheUsualWheelProject.Tests/TheUsualWheelProject.Tests.csproj reference TheUsualWheelProject/TheUsualWheelProject.csproj
   ```
3. Install Moq in the test project:
   ```
   dotnet add TheUsualWheelProject.Tests package Moq
   ```
4. Your solution should now look like:
   ```
   The Usual Wheel Project/
   ├── TheUsualWheelProject/          ← main app
   └── TheUsualWheelProject.Tests/    ← test project
       └── Services/
           ├── WheelServiceTests.cs
           └── MovieServiceTests.cs
   ```

### What to test

Focus test coverage on the **Services layer** — that is where your business logic lives.
Repositories are harder to unit test because they talk to a real database; skip those
for now or test them with an in-memory SQLite connection if needed later.

| Layer | Test? | Why |
|---|---|---|
| ViewModels | Optional | Mostly wiring, low logic density |
| Services | Yes | Business rules, elimination logic, last-standing check |
| Repositories | Skip for now | Requires DB setup, integration test territory |
| TmdbService | Skip / mock | External API, not your logic |

### What to mock

Use Moq to mock your repository interfaces so your service tests never touch a real
database. This is exactly why you defined `IMovieRepository` and `IWheelRepository`
interfaces — they make services testable in isolation.

For example, a `WheelService` test would create a `Mock<IWheelRepository>`, set up
its return values, pass it into `WheelService`, call the method under test, and
assert the result.

### Examples of good test cases

- `WheelService.ResetWheel` calls `SetEliminatedAsync` on all movies with `false`
- `WheelService.GetRemainingMovies` only returns non-eliminated entries
- `WheelService.IsLastStanding` returns `true` when exactly one movie is not eliminated
- `MovieService.AddMovieToWheel` does not add a duplicate if the movie already exists on that wheel

### Running tests

```
dotnet test
```

Run this from the solution root after any significant change to confirm nothing is broken.

---

## Gemini Prompt Template (Smoke Breaks)

Use this in your Gemini chat when generating break data for a new movie.
Copy the JSON output into a new `breaks_{slug}.json` file in `Resources/Raw/`.

```
Movie: [TITLE] ([YEAR])
Duration: [X minutes]

I need 2-4 smoke break timestamps for this movie, spread across the runtime.

For each break give me:
1. Timestamp in HH:MM:SS format
2. "Vibe check" — describe exactly where to pause (the scene, shot, or moment on screen)
3. "Recap" — 1-2 sentences summarising what has happened up to this point, for catching up after the break

Rules:
- Place breaks at natural lulls: after scene fades, quiet transitions, or emotional pauses
- Avoid interrupting action sequences or conversations
- Space them semi-evenly across the runtime
- Do not place one in the first 20 minutes or the last 15 minutes

Return as JSON using this exact schema:
{
  "movieTitle": "...",
  "tmdbId": ...,
  "breaks": [
    {
      "number": 1,
      "timestamp": "00:42:00",
      "vibeCheck": "...",
      "recap": "..."
    }
  ]
}
```

---

## Future Features (Post-MVP)

- **Gemini in-app:** Add a button on the movie details page that calls the Gemini API
  and auto-generates break suggestions, which can then be saved directly.
- **Peak rating classifier:** Tag movies above a rating threshold (e.g. Letterboxd ≥ 4.0)
  with a "Peak" badge on the wheel and details page.
- **Weighted spin:** Give higher-rated movies a slightly larger wheel segment so they
  have a marginally higher chance of being selected.
- **Google Play release:** If others want the app, package and publish it.
- **Import/Export:** Backup and share a wheel's full data (movies + breaks) as a single
  JSON export file.

---

## GitHub Issues — Suggested Structure

Create one issue per phase using the templates in `.github/ISSUE_TEMPLATE/`.
Suggested initial issues to create:

| # | Title | Label |
|---|---|---|
| 1 | Phase 1: Project setup and NuGet packages | feature |
| 2 | Phase 2: SQLite data models and DatabaseService | feature |
| 3 | Phase 3: TMDB API integration and movie search | feature |
| 4 | Phase 4: Smoke break JSON schema and loading | feature |
| 5 | Phase 5: Spinning wheel SkiaSharp control | feature |
| 6 | Phase 6: Movie details page with breaks display | feature |
| 7 | Phase 7: Stats and watch history | feature |
| 8 | Phase 8: Audio playback on wheel spin | feature |
| 9 | Setup test project (MSTest + Moq) | feature |
| 10 | Add Russell Crowe wheel seed data | movie-data |
| 11 | Add smoke breaks: Gladiator | movie-data |
| 12 | Add smoke breaks: Master and Commander | movie-data |

---

## Key Resources

| Resource | URL |
|---|---|
| .NET MAUI Docs | https://learn.microsoft.com/en-us/dotnet/maui/ |
| Blazor Hybrid Docs | https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui |
| CommunityToolkit.Mvvm | https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/ |
| SkiaSharp in MAUI | https://learn.microsoft.com/en-us/dotnet/maui/user-interface/graphics/skiasharp/ |
| sqlite-net-pcl | https://github.com/praeclarum/sqlite-net |
| TMDbLib | https://github.com/LordMike/TMDbLib |
| Plugin.Maui.Audio | https://github.com/jfversluis/Plugin.Maui.Audio |
| TMDB API Docs | https://developer.themoviedb.org/docs |
| System.Text.Json | https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview |
| Dapper | https://github.com/DapperLib/Dapper |
| Microsoft.Data.Sqlite | https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/ |
| MSTest | https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest |
| Moq | https://github.com/devlooped/moq |
