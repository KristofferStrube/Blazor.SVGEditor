using KristofferStrube.Blazor.SVGEditor.PathDataSequences;
using static System.Math;

namespace KristofferStrube.Blazor.SVGEditor.Extensions;

public static class PointExtensions
{
    public static bool WithinRangeOf(this (double x, double y) origin, (double x, double y) point, double range)
    {
        return Sqrt(Pow(origin.x - point.x, 2) + Pow(origin.y - point.y, 2)) <= range;
    }

    public static void UpdatePoints(this List<IPathInstruction> instructions, Func<(double, double), (double, double)> transformer, (double x, double y) scale)
    {
        instructions.ForEach(inst =>
        {
            inst.EndPosition = transformer(inst.EndPosition);
            if (inst is BaseControlPointPathInstruction controlPointInstruction)
            {
                controlPointInstruction.ControlPoints = controlPointInstruction.ControlPoints.Select(p => transformer(p)).ToList();
                controlPointInstruction.UpdateReflectionForInstructions();
            }
            else if (inst is EllipticalArcInstruction arcInstruction)
            {
                arcInstruction.Rx += arcInstruction.Rx * Cos(arcInstruction.XAxisRotation / 180 * PI) * (scale.x - 1);
                arcInstruction.Ry += arcInstruction.Ry * Sin(arcInstruction.XAxisRotation / 180 * PI) * (scale.y - 1);
            }
        });
    }
}
