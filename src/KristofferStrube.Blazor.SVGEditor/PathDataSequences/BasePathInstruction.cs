namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public abstract class BasePathInstruction : IPathInstruction
{
    public BasePathInstruction(bool Relative, IPathInstruction? PreviousInstruction)
    {
        this.Relative = Relative;
        this.PreviousInstruction = PreviousInstruction;
    }
    public IPathInstruction? PreviousInstruction { get; set; }
    public IPathInstruction? NextInstruction { get; set; }
    public bool ExplicitSymbol { get; set; } = true;
    public (double x, double y) StartPosition => PreviousInstruction is not null ? PreviousInstruction.EndPosition : (0, 0);
    public abstract (double x, double y) EndPosition { get; set; }
    public virtual List<(double x, double y)> SelectionPoints => [StartPosition];
    public abstract string AbsoluteInstruction { get; }
    public abstract string RelativeInstruction { get; }
    public abstract override string ToString();

    public virtual void SnapToInteger()
    {
        EndPosition = ((int)EndPosition.x, (int)EndPosition.y);
    }

    public bool Relative { get; set; }
}
