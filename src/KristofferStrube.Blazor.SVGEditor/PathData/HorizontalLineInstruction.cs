namespace KristofferStrube.Blazor.SVGEditor
{
    public class HorizontalLineInstruction : BasePathInstruction
    {
        public HorizontalLineInstruction(double x, bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction)
        {
            if (Relative)
            {
                this.x = StartPosition.x + x;
            }
            else
            {
                this.x = x;
            }
        }

        private double x { get; set; }

        public override (double x, double y) EndPosition
        {
            get { return (x, StartPosition.y); }
            set { x = value.x; }
        }

        public override string AbsoluteInstruction => "H";

        public override string RelativeInstruction => "h";

        public override string ToString() => (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? (x - StartPosition.x).AsString() : x.AsString());
    }
}
