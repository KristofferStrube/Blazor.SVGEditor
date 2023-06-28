using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class QuadraticBézierCurveInstruction : BaseControlPointPathInstruction
{
    public QuadraticBézierCurveInstruction(double x1, double y1, double x, double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        if (Relative)
        {
            ControlPoints.Add((StartPosition.x + x1, StartPosition.y + y1));
            X = StartPosition.x + x;
            Y = StartPosition.y + y;
        }
        else
        {
            ControlPoints.Add((x1, y1));
            X = x;
            Y = y;
        }
    }

    private double X { get; set; }

    private double Y { get; set; }

    public override (double x, double y) EndPosition
    {
        get => (X, Y);
        set { X = value.x; Y = value.y; }
    }

    public override string AbsoluteInstruction => "Q";

    public override string RelativeInstruction => "q";

    public override string ToString()
    {
        return (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") +
(Relative ?
$"{(ControlPoints[0].x - StartPosition.x).AsString()} {(ControlPoints[0].y - StartPosition.y).AsString()} {(X - StartPosition.x).AsString()} {(Y - StartPosition.y).AsString()}" :
$"{ControlPoints[0].x.AsString()} {ControlPoints[0].y.AsString()} {X.AsString()} {Y.AsString()}");
    }
}
