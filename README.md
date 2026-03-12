# The Usual Wheel

A .NET MAUI app for Android/iOS that combines movie databases and a wheel randomizer. Add movies to a wheel, spin it, and view detailed info about the selected movie. Built for personal use among friends.

## Features
- Multiple named wheels (e.g. "Russell Crowe", "Denzel Washington")
- Animated spinning wheel to select movies
- Movie elimination and tracking per wheel
- Movie details page with poster, synopsis, director, duration, and ratings
- Smoke break data loaded from per-movie JSON files
- TMDB movie search and metadata import
- Manual Letterboxd rating field
- Audio playback during wheel spin

## Tech Stack
- .NET MAUI (cross-platform mobile)
- SQLite + Dapper for data storage
- TMDbLib for movie metadata
- SkiaSharp for custom wheel graphics
- Plugin.Maui.Audio for audio
- CommunityToolkit.Mvvm for MVVM

## Attribution
Movie data and images provided by TMDB.
This product uses the TMDB API but is not endorsed or certified by TMDB.

## Usage
This app is for personal, non-commercial use only. Not intended for public distribution.

## Project Structure
```
TheUsualWheelProject/
├── Models/
├── Repositories/
├── Services/
├── ViewModels/
├── Views/
├── Controls/
└── Resources/
```

## Getting Started
1. Clone the repository
2. Build and run with:
   ```
   dotnet build -t:Run -f net9.0-android
   ```
3. Add your TMDB API key in a local `Constants.cs` file (not committed)

## License
Personal use only. See TMDB terms for API usage.
