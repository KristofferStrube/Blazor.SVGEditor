using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class LineInstruction : BasePathInstruction
{
    public LineInstruction(double x, double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        if (Relative)
        {
            X = StartPosition.x + x;
            Y = StartPosition.y + y;
        }
        else
        {
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

    public override string AbsoluteInstruction => "L";

    public override string RelativeInstruction => "l";

    public override string ToString()
    {
        return (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? $"{(X - StartPosition.x).AsString()} {(Y - StartPosition.y).AsString()}" : $"{X.AsString()} {Y.AsString()}");
    }
}
