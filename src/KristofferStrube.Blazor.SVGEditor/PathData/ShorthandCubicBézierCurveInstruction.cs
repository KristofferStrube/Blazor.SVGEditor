namespace KristofferStrube.Blazor.SVGEditor
{
    public class ShorthandCubicBézierCurveInstruction : BaseControlPointPathInstruction
    {
        public ShorthandCubicBézierCurveInstruction(double x2, double y2, double x, double y, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
        {
            if (Relative)
            {
                this.ControlPoints.Add((StartPosition.x + x2, StartPosition.y + y2));
                this.x = StartPosition.x + x;
                this.y = StartPosition.y + y;
            }
            else
            {
                this.ControlPoints.Add((x2, y2));
                this.x = x;
                this.y = y;
            }
        }

        private double x { get; set; }

        private double y { get; set; }

        public override (double x, double y) EndPosition
        {
            get { return (x, y); }
            set { x = value.x; y = value.y; }
        }

        public override string AbsoluteInstruction => "S";

        public override string RelativeInstruction => "s";

        public override string ToString() => (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") +
            (Relative ?
            $"{(ControlPoints[0].x - StartPosition.x).AsString()} {(ControlPoints[0].y - StartPosition.y).AsString()} {(x - StartPosition.x).AsString()} {(y - StartPosition.y).AsString()}" :
            $"{ControlPoints[0].x.AsString()} {ControlPoints[0].y.AsString()} {x.AsString()} {y.AsString()}");
    }
}
