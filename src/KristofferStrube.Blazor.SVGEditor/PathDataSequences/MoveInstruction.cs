using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class MoveInstruction : BasePathInstruction
{
    public MoveInstruction(double x, double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        if (Relative)
        {
            this.X = StartPosition.x + x;
            this.Y = StartPosition.y + y;
        }
        else
        {
            this.X = x;
            this.Y = y;
        }
    }

    private double X { get; set; }

    private double Y { get; set; }

    public override (double x, double y) EndPosition
    {
        get => (X, Y);
        set { X = value.x; Y = value.y; }
    }

    public override string AbsoluteInstruction => "M";

    public override string RelativeInstruction => "m";

    public override string ToString()
    {
        return (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? $"{(X - StartPosition.x).AsString()} {(Y - StartPosition.y).AsString()}" : $"{X.AsString()} {Y.AsString()}");
    }
}
