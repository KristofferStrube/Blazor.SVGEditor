using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class VerticalLineInstruction : BasePathInstruction
    {
        public VerticalLineInstruction(double y, IPathInstruction PreviousInstruction, bool Relative)
        {
            this.Relative = Relative;
            this.PreviousInstruction = PreviousInstruction;
            if (Relative)
            {
                this.y = StartPosition.y + y;
            }
            else
            {
                this.y = y;
            }
        }

        private double y { get; set; }

        public override (double x, double y) EndPosition
        {
            get { return (PreviousInstruction.EndPosition.x, y); }
            set { y = value.y; }
        }

        public override string AbsoluteInstruction => "V";

        public override string RelativeInstruction => "v";

        public override string ToString() => (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? (y - StartPosition.y).AsString() : y.AsString());
    }
}
