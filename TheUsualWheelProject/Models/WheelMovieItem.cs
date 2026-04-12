namespace TheUsualWheelProject.Models;

public class WheelMovieItem
{
    public required Movie Movie { get; set; }
    public required WheelMovie WheelMovie { get; set; }

    public bool IsWatched => WheelMovie.WatchedDate != null;
    public bool IsEliminated => WheelMovie.IsEliminated;
    
    // Active in the wheel session means it hasn't been eliminated from the current spin and is not watched yet
    public bool IsActive => !IsEliminated && !IsWatched;
}
