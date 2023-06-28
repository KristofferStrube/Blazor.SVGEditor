using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class HorizontalLineInstruction : BasePathInstruction
{
    public HorizontalLineInstruction(double x, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        X = Relative ? StartPosition.x + x : x;
    }

    private double X { get; set; }

    public override (double x, double y) EndPosition
    {
        get => (X, StartPosition.y);
        set => X = value.x;
    }

    public override string AbsoluteInstruction => "H";

    public override string RelativeInstruction => "h";

    public override string ToString()
    {
        return (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? (X - StartPosition.x).AsString() : X.AsString());
    }
}
