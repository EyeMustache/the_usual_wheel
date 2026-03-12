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