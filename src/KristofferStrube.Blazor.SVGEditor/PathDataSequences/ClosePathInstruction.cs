namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public class ClosePathInstruction : BasePathInstruction
{
    public ClosePathInstruction(bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction) { }

    public override (double x, double y) EndPosition
    {
        get
        {
            IPathInstruction prev = PreviousInstruction;
            while (prev is not (ClosePathInstruction or MoveInstruction))
            {
                prev = prev.PreviousInstruction;
            }
            return prev.EndPosition;
        }
        set
        {
        }
    }

    public IPathInstruction GetReferenceInstruction()
    {
        IPathInstruction prev = PreviousInstruction;
        while (prev is not (ClosePathInstruction or MoveInstruction))
        {
            prev = prev.PreviousInstruction;
        }
        while (prev is ClosePathInstruction closePath)
        {
            prev = closePath.GetReferenceInstruction();
        }
        return prev;
    }

    public override string AbsoluteInstruction => "Z";

    public override string RelativeInstruction => "z";

    public override string ToString()
    {
        return Relative ? RelativeInstruction : AbsoluteInstruction;
    }
}
