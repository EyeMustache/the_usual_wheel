using System.Data;
using System.Text.Json;
using Dapper;

namespace TheUsualWheelProject.Models;

public class WheelConfig
{
    public bool IsLastMovieStanding { get; set; } = true;
    public int SpinDurationSeconds { get; set; } = 5;
    public List<string> SliceColors { get; set; } = new List<string> { "#262626", "#FFA400" };
    public bool AllowDuplicates { get; set; } = false;
    public string AudioTrack { get; set; } = "price-is-right.mp3";
}

public class WheelConfigTypeHandler : SqlMapper.TypeHandler<WheelConfig>
{
    public override void SetValue(IDbDataParameter parameter, WheelConfig? value)
    {
        parameter.Value = value == null ? DBNull.Value : JsonSerializer.Serialize(value);
    }

    public override WheelConfig? Parse(object value)
    {
        if (value is string json)
        {
            return JsonSerializer.Deserialize<WheelConfig>(json) ?? new WheelConfig();
        }
        return new WheelConfig();
    }
}