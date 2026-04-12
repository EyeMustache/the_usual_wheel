using Dapper.Contrib.Extensions;

namespace TheUsualWheelProject.Models;

[Table("WheelMovie")]
public class WheelMovie : IModel
{
    [Key]
    public int Id { get; set; }
    public int WheelId { get; set; }
    public int MovieId { get; set; }
    public bool IsEliminated { get; set; }
    public DateOnly? WatchedDate { get; set; }
}
// In a home page have a message pop up kike welcome back, it then shows your most recently used wheel and the last movie you watched. (I was thinking about a crazy linq string that would go to all wheels, get their watched movies, filter by watched date and get the most recent but after thinking about it it would could be easier to simply add a last spin to a wheel in the database maybe based of their  movies? so that you can simply only compare wheels to their dates already prefiltered? or is there a better approach?)