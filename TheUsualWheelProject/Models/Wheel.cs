using Dapper.Contrib.Extensions;

namespace TheUsualWheelProject.Models;

[Table("Wheel")]
public class Wheel : IModel, IComparable<Wheel>
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public WheelConfig Configuration { get; set; } = new WheelConfig(); 
    
    public int CompareTo(Wheel? other)
    {
        if (other == null) return 1;
        int idComparison = Id.CompareTo(other.Id);
        if (idComparison != 0) return idComparison;
        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is not Wheel other) return false;
        return Id == other.Id && string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    public static bool operator ==(Wheel? left, Wheel? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Wheel? left, Wheel? right)
    {
        return !(left == right);
    }
}