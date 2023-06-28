namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public interface IPathInstruction
{
    public IPathInstruction? PreviousInstruction { get; set; }
    public IPathInstruction? NextInstruction { get; set; }
    public bool ExplicitSymbol { get; set; }
    public (double x, double y) StartPosition { get; }
    public (double x, double y) EndPosition { get; set; }
    public List<(double x, double y)> SelectionPoints { get; }
    public string AbsoluteInstruction { get; }
    public string RelativeInstruction { get; }
    public string ToString();
    public void SnapToInteger();
    public bool Relative { get; set; }
}
