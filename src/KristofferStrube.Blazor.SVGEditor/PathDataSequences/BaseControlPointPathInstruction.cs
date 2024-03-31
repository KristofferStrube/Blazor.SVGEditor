namespace KristofferStrube.Blazor.SVGEditor.PathDataSequences;

public abstract class BaseControlPointPathInstruction : BasePathInstruction
{
    public BaseControlPointPathInstruction(bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
    {
        UpdateReflectedPreviousInstructionsLastControlPoint();
    }
    public List<(double x, double y)> ControlPoints { get; set; } = [];
    public override List<(double x, double y)> SelectionPoints => base.SelectionPoints.Concat(ControlPoints).ToList();

    public (double x, double y) ReflectedPreviousInstructionsLastControlPoint
    {
        get => _reflectedPreviousInstructionsLastControlPoint;
        set
        {
            _reflectedPreviousInstructionsLastControlPoint = value;
            if (((this is ShorthandCubicBézierCurveInstruction && PreviousInstruction is CubicBézierCurveInstruction or ShorthandCubicBézierCurveInstruction) ||
                (this is ShorthandQuadraticBézierCurveInstruction && PreviousInstruction is QuadraticBézierCurveInstruction or ShorthandQuadraticBézierCurveInstruction)) &&
                 PreviousInstruction is BaseControlPointPathInstruction controlPointInstruction
            )
            {
                if (controlPointInstruction.ControlPoints.Count != 0)
                {
                    controlPointInstruction.ControlPoints[^1] = ((StartPosition.x * 2) - value.x, (StartPosition.y * 2) - value.y);
                }
                else
                {
                    controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint = ((StartPosition.x * 2) - value.x, (StartPosition.y * 2) - value.y);
                }
            }
            else
            {
                _reflectedPreviousInstructionsLastControlPoint = StartPosition;
            }
            UpdateReflectionForInstructions();
        }
    }

    public void UpdateReflectionForInstructions()
    {
        UpdateReflectedPreviousInstructionsLastControlPoint();
        if (NextInstruction is BaseControlPointPathInstruction reflectedControlPointInstruction)
        {
            reflectedControlPointInstruction.UpdateReflectionForInstructions();
        }
    }

    private void UpdateReflectedPreviousInstructionsLastControlPoint()
    {
        _reflectedPreviousInstructionsLastControlPoint = ((this is ShorthandCubicBézierCurveInstruction && PreviousInstruction is CubicBézierCurveInstruction or ShorthandCubicBézierCurveInstruction) ||
                (this is ShorthandQuadraticBézierCurveInstruction && PreviousInstruction is QuadraticBézierCurveInstruction or ShorthandQuadraticBézierCurveInstruction)) &&
                 PreviousInstruction is BaseControlPointPathInstruction controlPointInstruction
            ? controlPointInstruction.ControlPoints.Count != 0
                ? ((double x, double y))((StartPosition.x * 2) - controlPointInstruction.ControlPoints[^1].x, (StartPosition.y * 2) - controlPointInstruction.ControlPoints[^1].y)
                : ((double x, double y))((StartPosition.x * 2) - controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint.x, (StartPosition.y * 2) - controlPointInstruction.ReflectedPreviousInstructionsLastControlPoint.y)
            : StartPosition;
    }

    private (double x, double y) _reflectedPreviousInstructionsLastControlPoint;

    public override void SnapToInteger()
    {
        base.SnapToInteger();
        ControlPoints = ControlPoints.Select(c => ((double)(int)c.x, (double)(int)c.y)).ToList();
    }
}
