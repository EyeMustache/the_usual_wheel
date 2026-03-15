using Dapper.Contrib.Extensions;

namespace TheUsualWheelProject.Models;

[Table("WatchProvider")]
public class WatchProvider : IModel
{
    [Key]
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public required string Name { get; set; }
    public required string LogoUrl { get; set; }
    public int DisplayPriority { get; set; }
}
