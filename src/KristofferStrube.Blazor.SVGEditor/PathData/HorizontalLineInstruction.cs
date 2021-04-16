using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class HorizontalLineInstruction : BasePathInstruction
    {
        public HorizontalLineInstruction(double x, IPathInstruction PreviousInstruction, bool Relative)
        {
            this.Relative = Relative;
            this.PreviousInstruction = PreviousInstruction;
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
