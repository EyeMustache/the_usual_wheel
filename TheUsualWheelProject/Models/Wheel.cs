using Dapper.Contrib.Extensions;

namespace TheUsualWheelProject.Models;

[Table("Wheel")]
public class Wheel : IModel
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}