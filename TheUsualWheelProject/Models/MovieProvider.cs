using Dapper.Contrib.Extensions;

namespace TheUsualWheelProject.Models;

[Table("MovieProvider")]
public class MovieProvider : IModel
{
    [Key]
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int ProviderId { get; set; }
    public required string Type { get; set; }
}
