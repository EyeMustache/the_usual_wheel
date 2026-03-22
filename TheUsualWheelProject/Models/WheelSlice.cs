using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Models
{
    public record WheelSlice(
        Movie Movie,
        string PathData,
        string FillColor,
        string TextColor,
        string Label,
        double LabelX,
        double LabelY,
        double TextRotation
    );
}
