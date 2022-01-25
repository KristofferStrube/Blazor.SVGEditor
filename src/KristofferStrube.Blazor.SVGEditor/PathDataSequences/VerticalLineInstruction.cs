using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class VerticalLineInstruction : BasePathInstruction
{
    public VerticalLineInstruction(double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        this.Relative = Relative;
        if (Relative)
        {
            this.Y = StartPosition.y + y;
        }
        else
        {
            this.Y = y;
        }
    }

    private double Y { get; set; }

    public override (double x, double y) EndPosition
    {
        get => (PreviousInstruction.EndPosition.x, Y);
        set => Y = value.y;
    }

    public override string AbsoluteInstruction => "V";

    public override string RelativeInstruction => "v";

    public override string ToString()
    {
        return (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? (Y - StartPosition.y).AsString() : Y.AsString());
    }
}
