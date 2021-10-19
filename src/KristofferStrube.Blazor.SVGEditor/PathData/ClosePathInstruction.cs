namespace KristofferStrube.Blazor.SVGEditor
{
    public class ClosePathInstruction : BasePathInstruction
    {
        public ClosePathInstruction(bool Relative, IPathInstruction PreviousInstruction) : base(Relative, PreviousInstruction) { }

        public override (double x, double y) EndPosition
        {
            get
            {
                var prev = PreviousInstruction;
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
            var prev = PreviousInstruction;
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

        public override string ToString() => Relative ? RelativeInstruction : AbsoluteInstruction;
    }
}
