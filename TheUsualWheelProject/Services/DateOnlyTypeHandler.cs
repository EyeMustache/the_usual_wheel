using System.Data;
using Dapper;

namespace TheUsualWheelProject.Services;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value.ToString("yyyy-MM-dd");

    public override DateOnly Parse(object value)
        => DateOnly.Parse((string)value);
}