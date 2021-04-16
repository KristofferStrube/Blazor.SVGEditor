using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class LineInstruction : BasePathInstruction
    {
        public LineInstruction(double x, double y, IPathInstruction PreviousInstruction, bool Relative)
        {
            this.Relative = Relative;
            this.PreviousInstruction = PreviousInstruction;
            if (Relative)
            {
                this.x = StartPosition.x + x;
                this.y = StartPosition.y + y;
            }
            else
            {
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

        public override string AbsoluteInstruction => "L";

        public override string RelativeInstruction => "l";

        public override string ToString() => (ExplicitSymbol ? $"{(Relative ? RelativeInstruction : AbsoluteInstruction)} " : "") + (Relative ? $"{(x - StartPosition.x).AsString()} {(y - StartPosition.y).AsString()}" : $"{x.AsString()} {y.AsString()}");
    }
}
